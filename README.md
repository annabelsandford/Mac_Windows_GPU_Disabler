# Mac GPU Disabler for Windows 11 / 10

A GPU disabling tool for Macbook Pro's (2011+) with faulty GPU's (ATI/AMD Radeon; nVidia) running Windows 10 or 11.

## Prerequisites

* .NET Framework 4.7.2 (Already present on Windows 11)
* [Download the latest GPU_Disabler here](https://github.com/annabelsandford/Mac_Windows_GPU_Disabler/releases/)

## Usage

* Step #1: Unzip anywhere (can also run from a USB drive)
* Step #2: Run GUI_Disabler_netF.exe as administrator
* Step #3: There is no third step

## Stuff that's good to know
a) GUI_Disabler_netF.exe installs itself + all dependencies to C:\GPU_Disabler and automatically creates a shortcut to autostart. If you already have a GPU_Disabler folder with an old version but want the new version to take its place, delete C:\GPU_Disabler and it will install itself back to the directory. It also updates the autostart shortcut then.

b) The GPU_Disabler will automatically scan and get rid of the MacHALDriver.sys because it's the major reason 2011 MBP's bluescreen on Windows 11. There are no downsides to killing access to the driver.

## Checklist
- [X] Check for MacHALDriver.sys
- [X] Self installation
- [X] Automated audio fix

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[GNU General Public License v3.0](https://github.com/annabelsandford/Mac_Windows_GPU_Disabler/blob/main/LICENSE)
