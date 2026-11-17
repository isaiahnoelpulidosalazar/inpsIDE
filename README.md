# inpsIDE

A lightweight IDE for Windows made by inpsStudio, written in C# and Windows Forms on .NET 10.

Create, edit and run projects in several languages from one window. Each project type can bundle its own
language runtime, so there is nothing else to install.

## Features

- Project templates grouped by language (currently **PHP**, **Python** and **Node.js**), including a PHP web app, a Python Flask web app and a Node.js web app
- Bundled runtimes: PHP, Python and Node.js are extracted into each project, so you don't need to install them yourself
- Package manager support: run **pip** commands in Python projects and **npm** commands in Node.js projects from the Run menu (Ctrl+Shift+P)
- Code editor with line numbers and syntax highlighting (light theme)
- Project tree with create / delete / refresh
- Interactive console: output is streamed live and you can type input for programs that read from stdin
- Built-in web server support (PHP built-in server, Flask, Node.js `http` server)
- Single standalone executable, no .NET installation required

## Download

inpsIDE is available from the Microsoft Store:

**[Get inpsIDE from the Microsoft Store](https://apps.microsoft.com/detail/9nlzfwgck2m6)**

Updates are delivered through the Store.

## Building from source

Requirements: Windows, the .NET 10 SDK.

```powershell
git clone https://github.com/isaiahnoelpulidosalazar/inpsIDE.git
cd inpsIDE
dotnet build -c Release
```

To bundle the PHP and Node.js runtimes, place these zips in `Resources\` before building
(see `Resources\README.txt`):

- **PHP:** the *PHP Thread Safe x64* Windows zip, for example `php-8.5.0-Win32-vs17-x64.zip`
- **Node.js:** the *Windows Binary (.zip)* x64 build from nodejs.org, for example `node-v22.11.0-win-x64.zip`

Every `.zip` in `Resources\` is embedded at build time. The file name must contain `php` or `node` so inpsIDE can
tell them apart. The Python runtime is provided by the `inpsNuGet` package.

## Using pip and npm

Open a Python or Node.js project and choose **Run > Run pip Command...** or **Run > Run npm Command...**
(Ctrl+Shift+P). Type the arguments, for example `install requests` or `install express`, and the command runs in the
console using the project's own bundled runtime.

- **Python:** pip is installed into the project's Python runtime the first time you use it (internet required).
- **Node.js:** npm comes with the bundled Node.js zip. Packages are installed into the project's `node_modules` folder.

## Adding a language

Each language is one class in `Languages\`. Derive from `LanguageSupport`, describe its templates, runtime and
run commands, then add it to `LanguageRegistry`. See `Languages\LanguageSupport.cs` and the existing languages
for examples. A language can also expose a package manager through `PackageManagerName`,
`PreparePackageManager` and `CreatePackageCommand`.

## Privacy policy

inpsIDE does not collect, store or transmit any personal data, and it contains no telemetry, analytics,
advertising or crash reporting.

Specifically:

- **No data leaves your computer because of inpsIDE itself.** The program does not transfer any information to other
  networked systems unless specifically requested by the user or the person installing or operating it.
- **Local data only.** inpsIDE stores one file on your computer: the list of recently opened projects
  (project names and folder paths) in `%AppData%\inpsIDE\recent.json`. Delete this file at any time to clear it.
  Your project files stay in the folders you choose.
- **Network access only when you ask for it.** The only situations where inpsIDE itself triggers network traffic are
  when you install packages:
  - Creating a project from a template that installs packages (for example the Python Flask template runs
    `pip install flask`) downloads them from the Python Package Index (PyPI).
  - Running a pip command, or installing pip into a Python project for the first time, downloads from PyPI.
  - Running an npm command such as `npm install` downloads packages from the npm registry.

  These are direct results of your action, and your IP address and request are then handled by PyPI or npm under
  their own policies.
- **Local web servers.** When you start a project's web server it listens on `127.0.0.1` (your own computer only), and
  inpsIDE opens your default browser to that local address.
- **Programs you run.** Code that you write and run from inpsIDE, including scripts of packages you install, is
  executed with your user permissions and may do anything that code does, including accessing the network. That
  behaviour is controlled by you, not by inpsIDE.
- **Downloading inpsIDE.** Downloads and updates through the Microsoft Store are handled by Microsoft under the
  [Microsoft Privacy Statement](https://privacy.microsoft.com/privacystatement).

If this policy changes, the change will be described in the Store release notes and in this file.

## Contributing and issues

Bug reports and pull requests are welcome via
[GitHub Issues](https://github.com/isaiahnoelpulidosalazar/inpsIDE/issues).

## License

Released under the [MIT License](LICENSE). Bundled third-party runtimes keep their own licenses:
PHP is distributed under the [PHP License](https://www.php.net/license/), Python under the
[Python Software Foundation License](https://docs.python.org/3/license.html), and Node.js under the
[Node.js License (MIT)](https://github.com/nodejs/node/blob/main/LICENSE).