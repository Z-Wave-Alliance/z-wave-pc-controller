// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// IMA network map: nodes placed on the layout, dragged, and selected as in the WPF build's DrawCanvas
    /// (a click selects a node as source and turns the previous source into the destination, Ctrl/Shift click
    /// toggles, dragging on the background selects by rectangle, Ctrl+A selects all). Alt + drag pans, Ctrl + wheel zooms.
    /// </summary>
    public partial class IMAFullNetworkView : UserControl
    {
        private const double MaxLeft = 1460;
        private const double MaxTop = 960;
        private const double NodeSize = 40;
        private const double SnapDistance = 20;

        private IMAFullNetworkViewModel _viewModel;
        private readonly List<IMADeviceInfo> _selection = new List<IMADeviceInfo>();
        private IMADeviceInfo _sourceItem;
        private Point? _dragStart;
        private bool _isSnap;
        private Point? _selectionStart;
        private Point? _panStart;

        public IMAFullNetworkView()
        {
            InitializeComponent();
            surface.PointerPressed += OnSurfacePointerPressed;
            surface.PointerMoved += OnSurfacePointerMoved;
            surface.PointerReleased += OnSurfacePointerReleased;
            surface.KeyDown += OnSurfaceKeyDown;
            scrollViewer.AddHandler(PointerPressedEvent, OnPanPressed, RoutingStrategies.Tunnel);
            scrollViewer.AddHandler(PointerMovedEvent, OnPanMoved, RoutingStrategies.Tunnel);
            scrollViewer.AddHandler(PointerReleasedEvent, OnPanReleased, RoutingStrategies.Tunnel);
            scrollViewer.AddHandler(PointerWheelChangedEvent, OnZoomWheel, RoutingStrategies.Tunnel);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (_viewModel != null)
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = DataContext as IMAFullNetworkViewModel;
            if (_viewModel != null)
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _selection.Clear();
            _sourceItem = null;
            UpdateBackground();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BackgroundImagePath" || e.PropertyName == "BackgroundColor" || e.PropertyName == "UseBackgroundColor")
                UDispatcher.RunOnUIThread(UpdateBackground);
        }

        private void UpdateBackground()
        {
            if (_viewModel == null)
                return;
            try
            {
                if (_viewModel.UseBackgroundColor && _viewModel.BackgroundColor != null && _viewModel.BackgroundColor.Length == 4)
                {
                    var argb = _viewModel.BackgroundColor;
                    canvasBackground.Background = new SolidColorBrush(Color.FromArgb(argb[0], argb[1], argb[2], argb[3]));
                    return;
                }
                Bitmap image = !string.IsNullOrEmpty(_viewModel.BackgroundImagePath) && File.Exists(_viewModel.BackgroundImagePath)
                    ? new Bitmap(_viewModel.BackgroundImagePath)
                    : new Bitmap(AssetLoader.Open(new Uri("avares://ZWaveControllerUI/Images/Office1.png")));
                canvasBackground.Background = new ImageBrush(image) { Stretch = Stretch.Uniform };
            }
            catch (Exception)
            {
                canvasBackground.Background = Brushes.White;
            }
        }

        private IEnumerable<IMADeviceInfo> Nodes => _viewModel?.Items?.OfType<IMADeviceInfo>() ?? Enumerable.Empty<IMADeviceInfo>();

        #region Selection

        private void AddToSelection(IMADeviceInfo item)
        {
            item.IsSelected = true;
            if (!_selection.Contains(item))
                _selection.Add(item);
        }

        private void RemoveFromSelection(IMADeviceInfo item)
        {
            item.IsSelected = false;
            _selection.Remove(item);
        }

        private void ClearSelection()
        {
            foreach (var item in _selection)
                item.IsSelected = false;
            _selection.Clear();
        }

        private void SelectItem(IMADeviceInfo item)
        {
            ClearSelection();
            AddToSelection(item);
            if (item.Id != _viewModel.SourceNode)
            {
                if (_sourceItem != null)
                {
                    _sourceItem.IsSelectedAsSource = false;
                    _viewModel.DestinationNode = _sourceItem.Id;
                }
                _sourceItem = item;
                item.IsSelectedAsSource = true;
                _viewModel.SourceNode = item.Id;
            }
        }

        private void SelectAll()
        {
            ClearSelection();
            foreach (var item in Nodes)
                AddToSelection(item);
        }

        #endregion

        #region Pointer

        private void OnSurfacePointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (_viewModel == null || !e.GetCurrentPoint(surface).Properties.IsLeftButtonPressed)
                return;
            surface.Focus();
            var position = e.GetPosition(surface);
            var item = (e.Source as StyledElement)?.DataContext as IMADeviceInfo;
            if (item != null)
            {
                if ((e.KeyModifiers & (KeyModifiers.Shift | KeyModifiers.Control)) != KeyModifiers.None)
                {
                    if (item.IsSelected)
                        RemoveFromSelection(item);
                    else
                        AddToSelection(item);
                }
                else if (!item.IsSelected)
                {
                    SelectItem(item);
                }
                _dragStart = position;
                _isSnap = true;
            }
            else
            {
                ClearSelection();
                _selectionStart = position;
            }
            e.Pointer.Capture(surface);
            e.Handled = true;
        }

        private void OnSurfacePointerMoved(object sender, PointerEventArgs e)
        {
            var position = e.GetPosition(surface);
            if (_dragStart.HasValue && _selection.Count > 0)
            {
                DragSelection(position - _dragStart.Value, ref position);
            }
            else if (_selectionStart.HasValue)
            {
                var rect = new Rect(_selectionStart.Value, position).Normalize();
                selectionRect.IsVisible = true;
                selectionRect.Margin = new Thickness(rect.X, rect.Y, 0, 0);
                selectionRect.Width = rect.Width;
                selectionRect.Height = rect.Height;
                foreach (var item in Nodes)
                {
                    var bounds = new Rect((ushort)item.TopLeft, item.TopLeft >> 16, NodeSize, NodeSize);
                    if (rect.Intersects(bounds))
                        AddToSelection(item);
                    else if (item.IsSelected)
                        RemoveFromSelection(item);
                }
            }
        }

        /// <summary>
        /// Moves the selected nodes, kept inside the canvas; small first movements are ignored (snap).
        /// </summary>
        private void DragSelection(Vector change, ref Point position)
        {
            double minLeft = double.MaxValue, minTop = double.MaxValue, maxLeft = double.MinValue, maxTop = double.MinValue;
            foreach (var item in _selection)
            {
                double left = (ushort)item.TopLeft;
                double top = item.TopLeft >> 16;
                minLeft = Math.Min(left, minLeft);
                minTop = Math.Min(top, minTop);
                maxLeft = Math.Max(left, maxLeft);
                maxTop = Math.Max(top, maxTop);
            }
            double deltaHorizontal = Math.Min(MaxLeft - maxLeft, Math.Max(0 - minLeft, change.X));
            double deltaVertical = Math.Min(MaxTop - maxTop, Math.Max(0 - minTop, change.Y));
            _isSnap = _isSnap && Math.Abs(deltaHorizontal) < SnapDistance && Math.Abs(deltaVertical) < SnapDistance;
            if (_isSnap)
                return;
            foreach (var item in _selection)
            {
                double newLeft = (ushort)item.TopLeft + deltaHorizontal;
                double newTop = (item.TopLeft >> 16) + deltaVertical;
                item.TopLeft = ((ushort)newTop << 16) + (ushort)newLeft;
            }
            _dragStart = _dragStart.Value + new Vector(deltaHorizontal, deltaVertical);
        }

        private void OnSurfacePointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _dragStart = null;
            _selectionStart = null;
            selectionRect.IsVisible = false;
            e.Pointer.Capture(null);
        }

        private void OnSurfaceKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                SelectAll();
                e.Handled = true;
            }
        }

        private void OnPanPressed(object sender, PointerPressedEventArgs e)
        {
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Alt))
                return;
            _panStart = e.GetPosition(scrollViewer);
            scrollViewer.Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Pointer.Capture(scrollViewer);
            e.Handled = true;
        }

        private void OnPanMoved(object sender, PointerEventArgs e)
        {
            if (!_panStart.HasValue)
                return;
            var position = e.GetPosition(scrollViewer);
            var delta = position - _panStart.Value;
            _panStart = position;
            scrollViewer.Offset = new Vector(Math.Max(0, scrollViewer.Offset.X - delta.X), Math.Max(0, scrollViewer.Offset.Y - delta.Y));
            e.Handled = true;
        }

        private void OnPanReleased(object sender, PointerReleasedEventArgs e)
        {
            if (!_panStart.HasValue)
                return;
            _panStart = null;
            scrollViewer.Cursor = Cursor.Default;
            e.Pointer.Capture(null);
            e.Handled = true;
        }

        private void OnZoomWheel(object sender, PointerWheelEventArgs e)
        {
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
                return;
            slider.Value = Math.Clamp(slider.Value + (e.Delta.Y > 0 ? 0.1 : -0.1), slider.Minimum, slider.Maximum);
            e.Handled = true;
        }

        #endregion
    }
}
