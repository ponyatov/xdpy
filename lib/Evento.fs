//! generic embedded project generation script in F#

// project metainfo
let APP   = "xdpy"
let TITLE = "raw XDP socket library for high-load networking"

let ABOUT = "
- Mellanox NIC tested only
- base language: Rust
- TODO: Python module
"

// mostly constant metainfo
let app = APP.ToLower()
let VERSION = "0.0.1"
let AUTHOR = "Dmitry Ponyatov"
let EMAIL = "dponyatov@gmail.com"
let YEAR = 2025
let LICENSE = "MIT"
let GITHUB = $"https://github.com/ponyatov/{app}"

// file generation
open System
open System.IO

let touch (path: string) : unit =
    if not (File.Exists(path)) then
        File.WriteAllText(path, "")

let mkdir (path: string) : unit =
    if not (Directory.Exists(path)) then
        Directory.CreateDirectory(path) |> ignore
    let giti = Path.Combine(path, ".gitignore")
    if not (File.Exists(giti)) then
        File.WriteAllText(giti,"!.gitignore\n")

let NewLines = List.reduce (fun a b -> $"{a}\n{b}")

let meld path = $"meld {path} ~/em/{path} &"
let spawn cmd = cmd

// env
let USER = Environment.UserName
let HOME = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)

// project dir
let CWD = $"{HOME}/{app}"
mkdir CWD
Directory.SetCurrentDirectory(CWD)

let README:unit = //
    File.WriteAllText ("README.md",$"# ![](doc/logo.png) `{APP}` {VERSION}
## {TITLE}

(c) {AUTHOR} <<{EMAIL}>> {YEAR} {LICENSE}

github: {GITHUB}
{ABOUT}")

let LICFILE:unit = //
    File.WriteAllText ("LICENSE",$"MIT License

Copyright (c) {YEAR} {AUTHOR} <{EMAIL}>

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the \"Software\"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
")

let giti:unit = //
    File.WriteAllText(".gitignore","""*~
*.swp
*.log
*.o
*.exe
node_modules/
/target/
*.pyc
lib64
!.gitignore
""")

// github repo
spawn $"git clone -o gh git@github.com:ponyatov/{app}.git {HOME}/{APP} ; cd {HOME}/{APP}"
spawn $"git remote add gh git@github.com:ponyatov/{app}.git"
spawn $"git remote add flic git@gitflic.ru:dponyatov/{app}.git"
spawn "git remote -v"
spawn $"git checkout --orphan {USER}"
spawn "ln -fs ../rc rc"
spawn $"git gui &"
spawn $"git add -A ; git commit -am \".\" ; git push -v -u gh {USER} ; pp"
let COMMIT = "git add -A ; git commit -am \".\" ; pp"

let bin:unit = //
    for d in ["bin"; "tmp"; "ref"] do
        mkdir d
        File.WriteAllText($"{d}/.gitignore","*\n!.gitignore\n")

let doc:unit = //
    mkdir "doc"
    File.WriteAllText($"doc/.gitignore","html/\n!.gitignore\n")
    doxy

let doxy: unit = //
    mkdir "doc"
    let LOGO = "cp ~/icons/control64.png doc/logo.png"
    let DOXY = "doxygen -l ; mv DoxygenLayout.xml doc/"
    meld "doc/DoxygenLayout.xml"
    File.WriteAllText (".doxygen",$"PROJECT_NAME           = \"{APP}\"
PROJECT_BRIEF          = \"{TITLE}\"
PROJECT_LOGO           = doc/logo.png
")
    meld ".doxygen"

let lib:unit = //
    mkdir "lib"
    File.WriteAllText($"lib/.gitignore", "python*/\n!.gitignore\n")
    File.WriteAllText($"lib/{app}.ini", """#!/usr/bin/env shebang

# line comment
/* block comment */

# numbers:
-01 +02.30 -4e+5 0xDeadBeef 0o750 0b1101

:init
    nop halt
    jmp init
    call forward

:forward
    ret

# booleans
true false
""")

let cpp: unit = //
    mkdir "inc" ; touch $"inc/{app}.hpp"
    mkdir "src" ; File.WriteAllText ($"src/{app}.cpp",$"#include \"{app}.hpp\"\n")
    // 
    File.WriteAllText ($"inc/{app}.hpp",$"#pragma once

#include <stdlib.h>
#include <stdio.h>
#include <assert.h>

extern int main(int argc, char *argv[]);
extern void arg(int argc, char *argv);

extern int yylex();
extern int yylineno;
extern char *yytext;
extern char *yyfile;
extern FILE *yyin;
extern int yyparse();
extern void yyerror(const char *msg);
#include \"{app}.yacc.hpp\"
")
    mkdir "src"
    let include = $"#include \"{app}.hpp\""
    File.WriteAllText ($"src/{app}.cpp",include + """

int main(int argc, char *argv[]) {
    arg(0, argv[0]);
    for (int i = 1; i < argc; i++) {
        arg(i, argv[i]);
        yyfile = argv[i];
        assert(yyin = fopen(yyfile, "r"));
        yyparse();
        fclose(yyin);
        yyfile = nullptr;
    }
    return 0;
}

void arg(int argc, char *argv) {  //
    fprintf(stderr, "arg[%i] = <%s>\n", argc, argv);
}
""")
    File.WriteAllText ($"src/{app}.lex","%{\n"+include+ """
char* yyfile = nullptr;
%}

%option noyywrap yylineno

%%
. {yyerror("");} // lexer error on any undetected char
""")
    File.WriteAllText ($"src/{app}.yacc","%{\n"+include+ """
%}

%defines %union { char c; char* s; int n; float f; }

%%
syntax:

%%
void yyerror(const char *msg) {
    fprintf(stderr, "\n\n%s:%i %s [%s]\n\n", yyfile, yylineno, msg, yytext);
    exit(-1);
}
""")

let cargo_config:unit = //
    mkdir ".cargo"
    File.WriteAllText (".cargo/config.toml","""[build]
target    = "x86_64-unknown-linux-gnu"
jobs      = 2

[target.x86_64-unknown-linux-gnu]
rustflags = ["--cfg", "feature=\"pc,linux\""]
linker    = "x86_64-linux-gnu-gcc"

[target.aarch64-unknown-linux-gnu]
rustflags = ["--cfg", "feature=\"opi800,linux\""]
linker    = "aarch64-linux-gnu-gcc"

[target.armv7-unknown-linux-gnueabihf]
linker    = "arm-linux-gnueabihf-gcc"

[target.wasm32-unknown-unknown]
linker    = "rust-lld"

[target.i686-pc-windows-gnu]
rustflags = ["--cfg", "feature=\"pc,i686,mingw32\""]
linker    = "i686-w64-mingw32-gcc"

[target.x86_64-pc-windows-gnu]
rustflags = ["--cfg", "feature=\"pc,i5,win64\""]
linker    = "x86_64-w64-mingw32-gcc"

[target.thumbv7m-none-eabi]
rustflags = ["--cfg", "feature=\"cortexm3\""]
linker    = "arm-none-eabi-gcc"
runner    = [
    'qemu-system-arm',
    '-machine','netduino2','-cpu','cortex-m3',
    '-nographic','-semihosting-config','enable=on,target=native',
    '-s','-S','-kernel'
]
rustflags = [
    "-C", "link-arg=-Tlink.x",
    "-C", "link-arg=-nostartfiles",
]

[target.thumbv7em-none-eabihf]
rustflags = ["--cfg", "feature=\"cortexm4\""]
linker    = "arm-none-eabi-gcc"
runner    = [
    'qemu-system-arm',
    '-machine','netduinoplus2','-cpu','cortex-m4',
    '-nographic','-semihosting-config','enable=on,target=native',
    '-s','-S','-kernel'
]
rustflags = [
    "-C", "link-arg=-Tlink.x",
    "-C", "link-arg=-nostartfiles",
]

[source.crates-io]
replace-with = 'ustc'

[source.ustc]
registry = "sparse+https://mirrors.ustc.edu.cn/crates.io-index/"
[source.tuna]
registry = "sparse+https://mirrors.tuna.tsinghua.edu.cn/crates.io-index/"
""")
    meld ".cargo/config.toml"

let rsmain: unit = //
    mkdir "src"
    touch "src/config.rs"
    File.WriteAllText ("src/lib.rs",$"pub mod {app};\n")
    touch $"src/{app}.rs"
    let main = """
fn main() {
    let argv: Vec<String> = std::env::args().collect();
    let _argc = argv.len();
    arg(0, &argv[0]);
    for (i, argv) in argv.iter().enumerate().skip(1) {
        arg(i, argv);
    }
}
"""
    let arg = """fn arg(argc: usize, argv: &str) {
    eprintln!("argv[{argc}] = {argv:?}");
}
"""
    File.WriteAllText ("src/main.rs",$"mod config;\nmod {app};\n{main}\n{arg}")

let rserver: unit = //
    File.WriteAllText ("src/server.rs",$"mod config;\nmod {app};\n")

let rust: unit = //
    rsmain
    cargo_config
    File.WriteAllText ( "Cargo.toml", $"\
[package]
name                    =  \"{app}\"
version                 =  \"{VERSION}\"
description             =  \"{TITLE}\"
authors                 = [\"{AUTHOR} <{EMAIL}>\"]
license                 =  \"{LICENSE}\"
repository              =  \"{GITHUB}\"
edition                 =  \"2024\"

[[bin]]
name                    = \"recv\"
path                    = \"src/recv.rs\"

[[bin]]
name                    = \"send\"
path                    = \"src/send.rs\"

[dependencies]
const_format            = \"0.2\"
nom                     = \"8.0\"

[target.'cfg(all(target_os = \"linux\"))'.dependencies]
libc    = \"0.2\"
memmap2 = \"0.9\"

[features]

# hw
pc              = [\"i5\"]
# cpu
i5              = [\"x86_64\"]
# arch
x86_64          = [\"linux\"]
# os
linux           = []
")

// sdl2    = {version = \"0.38\", features = [\"ttf\",\"image\"], optional = true}

// [target.'cfg(all(target_arch = \"arm\", target_os = \"none\"))'.dependencies]
// cortex-m          = \"0.7\"
// cortex-m-rt       = \"0.7\"
// panic-semihosting = \"0.6\"

// ")
    meld "Cargo.toml"

let html:unit = //
    mkdir "static"
    File.WriteAllText ("static/.gitignore","*.wasm\n!.gitignore\n")
    mkdir "static/cdn"
    File.WriteAllText ("static/cdn/.gitignore","*\n!.gitignore\n")
    touch "static/index.html"
    touch "static/css.css"
    touch "static/js.js"

let src:unit = //
    cpp
    rust
    html

let cross_ name = //
    mkdir $"{name}"
    mkdir $"{name}/inc"
    mkdir $"{name}/src"
    let hd = 
        match name with
        | s when s="hw" -> "/// @defgroup cross cross\n"
        | _ -> ""
    File.WriteAllText ($"{name}/inc/{name}.hpp",$"{hd}/// @defgroup {name} {name}\n/// @ingroup cross\n")
    File.WriteAllText ($"{name}/src/{name}.cpp",$"#include \"{name}.hpp\"\n")

let hw:unit = //
    cross_ "hw"

    for hw,cpu in [
        ("pc","i5");
        ("qemu386","i486"); ("retro","i686");
        ("rpi3","bcm2837"); ("rpi4","bcm2711"); ("rpi5","bcm2712"); ("opi800","rk3399");
        ("a7n8x","athlon")
        // ("pillf103","stm32f103c8"); ("f429disco","stm32f429zi");
        // ("netduinoplus2","stm32f405rg");
        // ("iskra","stm32f405rg"); ("f4disco","stm32f407vg");
        // ("esp8266","lx106"); ("esp32","lx106");
        ] do
            mkdir $"hw/{hw}"
            touch $"hw/{hw}/{hw}.kernel"
            File.WriteAllText ($"hw/{hw}/{hw}.mk",$"CPU = {cpu}\n")
            touch $"hw/{hw}/{hw}.cmake"
            mkdir $"hw/{hw}/inc"
            mkdir $"hw/{hw}/src"
            touch $"hw/{hw}/src/{hw}.cpp"
            File.WriteAllText ($"hw/{hw}/inc/{hw}.hpp",$"/// @defgroup {hw} {hw}\n/// @ingroup hw\n")

let cpu:unit = //
    cross_ "cpu"

    for cpu,arch in [
        ("i5","x86_64");
        ("i486","i386"); ("i686","i386");
        ("athlon","i386");
        ("bcm2837","aarch64"); ("bcm2711","aarch64"); ("bcm2712","aarch64"); ("rk3399","aarch64");
        // ("stm32f103c8","cortexm3"); ("stm32f429zi","cortexm4");
        // ("stm32f405rg","cortexm4"); ("stm32f407vg","cortexm4");
        // ("lx106","xtensa");
        ] do
            mkdir $"cpu/{cpu}"
            touch $"cpu/{cpu}/{cpu}.kernel"
            File.WriteAllText ($"cpu/{cpu}/{cpu}.mk",$"ARCH = {arch}\n")
            touch $"cpu/{cpu}/{cpu}.cmake"
            mkdir $"cpu/{cpu}/inc"
            mkdir $"cpu/{cpu}/src"
            File.WriteAllText ( $"cpu/{cpu}/inc/{cpu}.hpp",$"/// @defgroup {cpu} {cpu}\n/// @ingroup cpu\n")
            File.WriteAllText ( $"cpu/{cpu}/src/{cpu}.cpp",$"#include \"{cpu}.hpp\"\n")

let arch:unit = //
    cross_ "arch"

    for arch in [
        "x86_64";
        "i386";
        "aarch64";
        // "cortexm"; "cortexm3"; "cortexm4";
        // "xtensa";
        ] do
            mkdir $"arch/{arch}"
            touch $"arch/{arch}/{arch}.kernel"
            touch $"arch/{arch}/{arch}.uclibc"
            touch $"arch/{arch}/{arch}.mk"
            touch $"arch/{arch}/{arch}.cmake"
            mkdir $"arch/{arch}/inc"
            mkdir $"arch/{arch}/src"
            File.WriteAllText ( $"arch/{arch}/inc/{arch}.hpp",$"/// @defgroup {arch} {arch}\n/// @ingroup arch\n")
            File.WriteAllText ( $"arch/{arch}/src/{arch}.cpp",$"#include \"{arch}.hpp\"\n")

let os:unit = //
    cross_ "os"
    touch "os/linux/all.kernel"
    for os in [
        "linux";
        // "none";
        // "freertos";
        // "win32";
        // "rtos8266";
        // "idf"
    ] do
        mkdir $"os/{os}" ; touch $"os/{os}/{os}.mk" ; touch $"os/{os}/{os}.cmake"
        mkdir $"os/{os}/inc" ; mkdir $"os/{os}/src"
        File.WriteAllText ( $"os/{os}/inc/{os}.hpp",$"/// @defgroup {os} {os}\n/// @ingroup os\n")
        File.WriteAllText ( $"os/{os}/src/{os}.cpp",$"#include \"{os}.hpp\"\n")

let cross:unit = //
    hw
    cpu
    arch
    os

let vscode:unit = //
    mkdir ".vscode"
    let jsons = [
        "c_cpp_properties";
        "extensions";
        "launch";
        "settings";
        "tasks" ]
    for j in jsons do
        File.WriteAllText($".vscode/{j}.json","{\n}\n")
    settings ; tasks
    meld ".vscode"

let settings:unit = //
    File.WriteAllText ( ".vscode/settings.json","""{
    "files.exclude": {
        "doc/html": true, "**/node_modules/**": true,
    },
    "files.watcherExclude": {
        "bin/**": true, "tmp/**": true, "ref/**": true,
        "target/**": true, "obj/**": true,
    },
    "files.associations": {
        "*.mk": "makefile", "*.make": "makefile",
        "*.ld": "linkerscript", "*.ld.fix": "linkerscript",
        "*.ioc": "properties", "*.ocd": "properties",
        "*.s": "arm", "*.s.fix": "arm", "*.S": "arm",
        "*.kernel": "properties", "*.config": "properties",
        "*.service": "systemd-unit-file",
        "requirements.*": "properties",
        "*.ini": "bcx", "*.f": "bcx",
    },

    // editor
    "files.eol": "\n",
    "files.insertFinalNewline": true,
    "files.trimFinalNewlines": true,
    "editor.tabSize": 4,
    "editor.insertSpaces": true,
    "editor.detectIndentation": false,
    "editor.rulers": [80],
    "editor.lineNumbers": "on",
    "editor.formatOnSave": false,
    "workbench.tree.indent": 24,
    "editor.fontSize": 14,
    "explorer.autoReveal": false,
    "terminal.integrated.copyOnSelection": true,
    "files.autoSave": "afterDelay",
    "files.autoSaveDelay": 2222,
    // "git.enabled": false,

    // JavaScript
    "prettier.configPath"         : ".prettierrc",
    "prettier.requireConfig"      :  true,
    "json.format.enable"          :  true,

    // clang-format
    "clang-format.executable"     : "clang-format",
    "clang-format.fallbackStyle"  : "Google",
    "clang-format.style"          : "file",

}
""")

let extensions:unit = //
    File.WriteAllText ( ".vscode/extensions.json","""{
    "recommendations": [
        "stkb.rewrap",
        "ms-vscode.makefile-tools",
        "IBM.output-colorizer",
        // formatters
        "xaver.clang-format",
        "esbenp.prettier-vscode",
        "foxundermoon.shell-format",
        // Linux
        "ms-vscode-remote.remote-ssh",
        "coolbear.systemd-unit-file",
        // misc
        "usernamehw.errorlens",
        // C++
        "ms-vscode.cpptools",
        "jeff-hykin.better-cpp-syntax",
        "ms-vscode.cmake-tools",
        // parser
        "rreverser.ragel",
        "serghei-iakovlev.language-lemon",
        "daohong-emilio.yash",
        // embedded
        "basdp.language-gas-x86",
        "dan-c-underwood.arm",
        "zixuanwang.linkerscript",
        "ms-vscode.vscode-serial-monitor",
        // Python
        "ms-python.python",
        "ms-python.autopep8",
        // Rust
        "rust-lang.rust-analyzer",
        "tamasfe.even-better-toml",
        "vadimcn.vscode-lldb",
}
""")

let tasks:unit = //
    File.WriteAllText ( ".vscode/tasks.json","""{
    "version": "2.0.0",
    "tasks": [
        {
            "label"          : "project: install",
            "type"           : "shell",
            "command"        : "make install",
            "presentation"   : {"focus": true},
            "problemMatcher" : []
        },
        {
            "label"          : "project: update",
            "type"           : "shell",
            "command"        : "make update",
            "presentation"   : {"focus": true},
            "problemMatcher" : []
        },
        {
            "label"          : "git: checkout .vscode",
            "type"           : "shell",
            "command"        : "git checkout .vscode/settings.json",
            "presentation"   : {"showReuseMessage": false, "focus": false, "reveal": "silent", "close": true},
            "problemMatcher" : []
        },
        {
            "label"          : "AI: context",
            "type"           : "shell",
            "command"        : "make ai",
            "problemMatcher" : [],
            "presentation"   : {"showReuseMessage": true, "focus": true, "reveal": "silent", "close": false},
            "group"          : {"kind": "build", "isDefault": true}
        },
    ]
}
""")

meld ".vscode"

let dirs:unit = //
    bin
    doc
    lib
    src
    cross
    vscode

let mk: unit = //
    mkdir "mk"
    let makes = ["var";"version";"dir";"cross";"tool";"src";"all";"format";"rule";"doc";"rust";"python";"ts";"gz";"ref";"install";"merge";"ai"]
    for m in makes do
        touch $"mk/{m}.mk"
    File.WriteAllText("Makefile",
        makes |> List.map (fun m -> $"include mk/{m}.mk") |> NewLines)
    meld "mk"

let cmake: unit = //
    touch "CMakeLists.txt"
    touch "CMakePresets.json"
    mkdir "cmake"
    let cmakes = ["any_toolchain"; "x86_64-linux-gnu"; "arm-none-eabi";
        "xtensa-lx106-elf"; "aarch64-linux-gnu"; "i686-w64-mingw32";
        "syntax"; "FindLEMON"; "FindRAGEL"; "FindReadline";
        "version"; "src"; "install"; "cross"; "clean"]
    for cm in cmakes do
        touch $"cmake/{cm}.cmake"
    let TXT = "cp ~/em/CMakeLists.txt CMakeLists.txt"
    let PRESET = "meld CMakePresets.json ~/em/CMakePresets.json"
    meld "cmake"

let linux_ver = "6.12.41"
let linux:unit = //
    mkdir "root"
    for d in ["boot";"isolinux"] do
        mkdir $"root/{d}"
    File.WriteAllText ("root/isolinux/isolinux.cfg",$"""\
default boot
timeout 1
label boot
kernel /boot/vmlinuz-{linux_ver}-qemu386
append root=LABEL=qemu386 vga=0x312
""")

let apt:unit = //
    File.WriteAllText ("apt.Debian","""git make curl
code meld doxygen clang-format
g++ cmake gdb gdb-multiarch
flex bison libreadline-dev ragel lemon
python3 python3-venv python3-autopep8 python3-ply
dotnet-runtime-9.0 dotnet-sdk-9.0
qemu-system-arm
    gcc-arm-none-eabi openocd newlib-source dfu-util stlink-tools
qemu-system-x86
    g++-mingw-w64-i686
""")


let clang_format:unit = //
    File.WriteAllText (".clang-format","""BasedOnStyle: Google
IndentWidth:  4
TabWidth:     4
UseTab:       Never
ColumnLimit:  80
UseCRLF:      false

SortIncludes: false

AllowShortBlocksOnASingleLine: Always
AllowShortFunctionsOnASingleLine: All
""")

let prettierrc:unit = //
    File.WriteAllText (".prettierrc","""{
    "tabWidth"    : 4,
    "useTabs"     : false,
    "endOfLine"   : "lf",
    "singleQuote" : true,
    "semi"        : true,
    "printWidth"  : 80
}
""")

let editorconfig:unit = //
    File.WriteAllText (".editorconfig","""# fantomas config
indent_size          = 4
max_line_length      = 80
end_of_line          = lf
insert_final_newline = true
""")

let gitattributes:unit = //
    File.WriteAllText (".gitattributes","""* text=auto eol=lf

# All source code in UNIX format
*.c   text diff=cpp
*.cpp text diff=cpp
*.h   text diff=cpp
*.hpp text diff=cpp
*.s   text diff=cpp
*.ld  text diff=cpp

# Binary files
*.bin  binary
*.elf  binary
*.dfu  binary
*.png  binary
*.pdf  binary
*.doc  binary
*.docx binary

# Linux
*.sh      text eol=lf
*.rc      text eol=lf
*.service text eol=lf

# Windows/MSYS
*.bat text eol=crlf
*.ps* text eol=crlf
""")

let format: unit = //
    clang_format
    prettierrc
    editorconfig
    gitattributes

let fs:unit = //
    mkdir "lib"
    for f in ["Evento";"Sestoft";"Parser";"AST"] do
        touch $"lib/{f}.fs"
    touch "Evento.fsproj"

let files :unit = //
    dirs
    mk
    cmake
    giti
    apt
    format
    fs

let package:unit = //
    touch $"src/{app}.ts"
    File.WriteAllText ("package.json",$"{{
    \"name\"        : \"{app}\",
    \"version\"     : \"{VERSION}\",
    \"description\" : \"{TITLE}\",
    \"main\"        : \"src/{app}.ts\",
    \"directories\" : {{ \"doc\": \"doc\", \"src\": \"src\" }},
    \"scripts\": {{
        \"test\": \"echo \\\"Error: no test specified\\\" && exit 1\"
    }},
    \"author\": \"{AUTHOR} <{EMAIL}>\",
    \"license\": \"{LICENSE}\"
}}
")
    spawn "npm i -g deno typescript"

let requirements:unit = //
    touch $"src/{app}.ts"
    File.WriteAllText ("requirements.txt",$"
autopep8
ply
")
    meld "requirements.txt"

spawn COMMIT
