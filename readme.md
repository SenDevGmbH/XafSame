# XAF Standalone Model Editor (XafSame)

## Overview
XafSame is a standalone model editor for applications built using the eXpressApp Framework (XAF). It provides developers with a convenient way to edit and manage application models outside of the integrated development environment.

## Features
- **Standalone Model Editing**: Modify application models without launching the full application.
- **User-Friendly Interface**: Simplified and intuitive UI for efficient model editing.
- **Cross-Platform Compatibility**: Works seamlessly on Windows systems.

## Installation  

1. Visit the [Releases](https://github.com/SenDevGmbH/XafSame/releases) page of the GitHub repository.  
2. Download the latest release package.  
3. Extract the downloaded package to your desired location.  
4. Follow the usage instructions below to launch the standalone editor.


## Usage  

To start the editor, follow these steps:  

1. Choose the appropriate framework folder based on your project type:  
   - Use the `net48` folder for .NET Framework 4.8 projects.  
   - Use the `netcore` folder for .NET Core projects.  

2. Run the `SenDev.XafSame.exe` executable from the selected folder, providing the path to the `.xafml` file as a parameter. 
  

## How it Works  

1. The editor operates independently of the DevExpress version, ensuring compatibility across different versions.  
2. It utilizes an MSBuild binary log to determine the referenced assemblies and the specific DevExpress version. For this purpose, the editor runs MSBuild.  
3. During this process, a file named `<ProjectName>.csproj.binlog` is created in the project folder.  
4. MSBuild is only executed if the binary log file is outdated, optimizing performance and avoiding unnecessary operations.

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Submit a pull request with a detailed description of your changes.

## License
This project is licensed under the [MIT License](LICENSE).

## Contact
For questions or support, please contact [xafsame@sendev.de].
