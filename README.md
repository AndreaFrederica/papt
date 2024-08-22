# papt 
## APT-like and Pacman-like wrapper for yay/pacman on Arch Linux

#### Build:
```
clone https://github.com/AndreaFrederica/papt.git
cd papt
./build.sh
```

#### Usage: 
`
papt <command> [package] [-pacman] [--noconfirm|-y] [-debug]
`

#### Commands:
```
  update                Update package database only.
  upgrade [package]     Upgrade all installed packages or a specific package.
  install <package>     Install the specified package.
  remove <package>      Remove the specified package.
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
```
#### Options:
```
  -pacman              Force using pacman instead of yay.
  --noconfirm, -y      Automatically confirm all prompts (for pacman or yay).
  -debug               Show cli command before call packagemanager.
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
```
#### Note:
  This script is a wrapper for the yay AUR helper, designed to mimic APT and Pacman commands.
  If yay is not installed, pacman will be used automatically with a warning message.

For more information, visit:  
  https://github.com/Jguer/yay  
  https://wiki.archlinux.org/title/Pacman  
  https://github.com/AndreaFrederica/papt  