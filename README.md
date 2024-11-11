# papt

## papt is Pacman-like wrapper for aur-helper/pacman on Arch Linux / Windows

#### Build:

```
clone https://github.com/AndreaFrederica/papt.git
cd papt
./build.ps1
```

#### Usage:

`papt <command> [package] [-pacman] [--noconfirm|-y] [-debug] <-helper> [aur-helper]`

#### Commands:

```
update                Update package database only.
upgrade [package]     Upgrade all installed packages or a specific package.
install <package>     Install the specified package.
remove <package>      Remove the specified package.
autoremove <package>  Autoremove the specified package and its dependencies.
search <package>      Search for a package in the repositories.
show <package>        Display detailed information about the specified package.
list                  List all installed packages.
clean                 Clean the package cache to free up space.
help                  Show this help message.
```

#### Pacman-style Commands:

```
-Sy                   Update package database only.
-Syu                  Update package database and upgrade all installed packages.
-S <package>          Install the specified package.
-SyS <package>        Update package database and install the specified package.
-R <package>          Remove the specified package.
-Ss <package>         Search for a package in the repositories.
-Qi <package>         Display detailed information about the specified package.
-Q                    List all installed packages.
-Sc                   Clean the package cache.
-h                    Show this help message.
...                   Other pacman commands will call package-manager directly.
```

#### Options:

```
-pacman               Force using pacman instead of aur-helper.
--noconfirm, -y       Automatically confirm all prompts (for pacman or aur-helper).
-debug                Show cli command before call packagemanager.
-helper <aur-helper>  Use custom aur-helper.
```

#### Examples:

```
papt update
papt upgrade
papt upgrade vim
papt install vim
papt remove vim
papt search vim
papt show vim
papt list
papt clean
papt install vim --noconfirm
papt remove vim -pacman
papt remove vim -pacman -helper yay
papt remove vim -pacman -helper paru
```

#### Note:

This program is a wrapper for aur-helper, designed to mimic APT and Pacman commands.  
If yay/paru/custom-helper is not installed, pacman will be used automatically with a warning message.  
You can find papt.json5 in /home/papt. Edit papt.json5 to change the priority of aur-helper.
example

```
{
//? Default configuration
//! Configuration of the priority of the aur-helper, start with 1, the lower the value the higher the priority.
"yay": 2,
"paru": 1
}
```

Now papt can translate some package names. For example, `build-essential --> base-devel`.
On Windows, papt will first use pwsh/powershell.  
To use papt/pacman, Windows users should install Cygwin/MSYS2 and add `/user/bin` `/bin` ... dir to their system path.  
You can download MSYS2 from there https://www.msys2.org/

For more information, visit:  
https://github.com/Jguer/yay  
https://github.com/Morganamilo/paru  
https://wiki.archlinux.org/title/Pacman  
https://github.com/AndreaFrederica/papt
