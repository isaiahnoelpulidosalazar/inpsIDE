PHP
---
Drop the PHP 8.5 THREAD SAFE Windows zip in this folder, e.g.

    php-8.5.0-Win32-vs17-x64.zip

(from https://windows.php.net/download - "VS17 x64 Thread Safe" -> Zip).

Any *.zip in this folder is embedded into inpsIDE.exe at build time. The IDE picks the first
embedded zip whose name contains "php" and extracts it into <project>\php when a PHP project is created.

Node.js
-------
Drop the Node.js Windows x64 zip in this folder, e.g.

    node-v22.11.0-win-x64.zip

(from https://nodejs.org/en/download - "Windows Binary (.zip)").

The IDE picks the first embedded zip whose name contains "node" and extracts it into <project>\node when a
Node.js project is created. npm is part of that zip and is used by "Run npm Command...".

Python
------
Nothing to add here: the Python 3.13 runtime is embedded in the inpsNuGet package (PyCS class)
and is extracted into <project>\Python when a Python project is created.
"Run pip Command..." installs pip into the project's Python runtime on first use.

Other languages
---------------
See Languages\LanguageSupport.cs for how to add one.
