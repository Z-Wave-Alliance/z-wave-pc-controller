# Contributing

All types of contributions are encouraged and valued. 

Contributions are welcome in the form of pull requests.
All incoming pull requests should be tested on the developer's own fork beforehand.

# Guidelines

Reading existing documents might help to get your contribution smoothly accepted:

https://github.com/Z-Wave-Alliance/OSWG

# Testing code

Since [Z-Wave development is not (yet) public](https://github.com/Z-Wave-Alliance/OSWG/discussions/41)

This chapter explains how to deal with permission issues between related projects.

To test your code, first, create a fork of this repository.
 
Then, generate a personal access token.\
Click [here](https://github.com/settings/tokens) or navigate to
*Settings/Developer/Personal access tokens/Tokens (classic)*, click on
"Generate new token" and select "Generate new token (classic)".\
On the next page, select the "repo" scope (Full control of private
repositories) and click on "Generate" at the bottom of the page.\
Take note of the generated token.

Open the settings of your forked repository and navigate to
*Security/Secrets and variables/Actions*.\
Click on "New repository secret".\
Enter "GH_ZWAVE_ACCESS_TOKEN" as the name and paste the generated token from the previous step
into the "Secret" field (without any newline characters at the end).

Now you should be able to run the tests on your own fork.

## Legal info

SPDX-FileCopyrightText: 2024 Z-Wave-Alliance <https://z-wavealliance.org/>

SPDX-License-Identifier: BSD-3-Clause
