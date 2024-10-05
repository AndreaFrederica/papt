# papt 
## papt是一个apt风格的pacman/aur-helper包装器，运行在 Arch Linux / Windows上。

#### 构建:
```
clone https://github.com/AndreaFrederica/papt.git
cd papt
./build.ps1
```

#### 用法:
`
papt <command> [package] [-pacman] [--noconfirm|-y] [-debug] <-helper> [aur-helper]
`

#### 指令:
```
  update                仅更新软件包数据库。
  upgrade [package]     升级所有已安装的软件包或特定软件包。
  install <package>     安装指定软件包。
  remove <package>      删除指定软件包。
  autoremove <package>  自动删除指定软件包和依赖。
  search <package>      在软件仓库中搜索软件包。
  show <package>        显示指定软件包的详细信息。
  list                  列出所有已安装的软件包。
  clean                 清理软件包缓存以释放空间。
  help                  显示此帮助信息。
 ```

#### Pacman风格的指令:
```
  -Sy                   仅更新软件包数据库。
  -Syu                  更新软件包数据库并升级所有已安装的软件包。
  -S <package>          安装指定软件包。
  -SyS <package>        更新软件包数据库并安装指定软件包。
  -R <package>          删除指定软件包。
  -Ss <package>         在软件仓库中搜索软件包。
  -Qi <package>         显示指定软件包的详细信息。
  -Q                    列出所有已安装的软件包。
  -Sc                   清理软件包缓存。
  -h                    显示此帮助信息。
  ...                   其他pacman命令会直接通过包管理器调用。
```
#### 选项:
```

  -pacman               强制使用 pacman 而不是 aur-helper。
  -noconfirm, -y        自动确认所有提示（pacman 或 aur-helper）。
  -debug                在调用包管理器之前显示 cli 命令。
  -helper <aur-helper>  使用自定义的 aur-helper
```

#### 用例:
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
#### 注记:
  该程序是 aur-helper 的包装器，旨在模仿 APT 和 Pacman 命令。
  如果未安装 yay/paru/custom-helper，则会自动使用 pacman，并给出警告信息。  
  现在，papt 可以转换一些软件包名称。例如，`build-essential --> base-devel`。
  在 Windows 上，papt 会首先使用 pwsh/powershell。  
  要使用 papt/pacman，Windows 用户应安装 Cygwin/MSYS2，并在Path中添加 `/user/bin``/bin` ... 目录。  
  你可以从这里下载 MSYS2 https://www.msys2.org/

更多信息，请访问  
  https://github.com/Jguer/yay  
  https://github.com/Morganamilo/paru  
  https://wiki.archlinux.org/title/Pacman  
  https://github.com/AndreaFrederica/papt  