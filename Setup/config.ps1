# SPDX-License-Identifier: BSD-3-Clause
# SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
# *********************************  config.ps1  **********************************
# Z-Wave, the wireless language.
# Copyright (c) 2018 Silicon Labs, Inc. All rights reserved.
#
# This source file is subject to the terms and conditions of the
# Silicon Labs Software License Agreement which restricts the manner
# in which it may be used.
#
# ---------------------------------------------------------------------------------
#
#  Description: PowerShell script file for patching WIX config file with product 
#				version and package id uder specified WIX project folder
#
#  Author:   Oleksandr Boiko
#  Last Changed By:  $Author: obo $
#  Revision:         $Revision: 0 $
#  Last Changed:     $Date: 2016-03-04 $
#
# usage:
#  from powershell.exe prompt:
#     .\config.ps1 <product version> <package guid>(optional)
#
# **********************************************************************************

function Usage
{
	Write-Output "Usage: ";
	Write-Output "     .\config.ps1 <product version> <package guid>(optional)";
	Write-Output ""
}

if ($args.Length -lt 1)
{
	Write-Error -Message "Error: Not enough input arguments." -Category InvalidArgument
	Usage;
	Exit(1)
}

$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
$filePath = $PSScriptRoot + "/Product.wxs"
$file = gi $filePath 
$xmlProduct = [xml](Get-Content $file)
$xmlProduct.Wix.Product.Version = $args[0]

if ($args.Length -eq 2)
{
	$xmlProduct.Wix.Product.Package.Id = $args[1]
}
$xmlProduct.Save($file.FullName)
