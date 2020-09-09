# hsync

```
hsync 2020.6.7
Build Date: 2020년 6월 7일 일요일
                                         /T /I
                              / |/ | .-~/
                          T\ Y  I  |/  /  _
         /T               | \I  |  I  Y.-~/
        I l   /I       T\ |  |  l  |  T  /
     T\ |  \ Y l  /T   | \I  l   \ `  l Y
 __  | \l   \l  \I l __l  l   \   `  _. |
 \ ~-l  `\   `\  \  \\ ~\  \   `. .-~   |
  \   ~-. "-.  `  \  ^._ ^. "-.  /  \   |
.--~-._  ~-  `  _  ~-_.-"-." ._ /._ ." ./
 >--.  ~-.   ._  ~>-"    "\\   7   7   ]
^.___~"--._    ~-{  .-~ .  `\ Y . /    |
 <__ ~"-.  ~       /_/   \   \I  Y   : |
   ^-.__           ~(_/   \   >._:   | l______
       ^--.,___.-~"  /_/   !  `-.~"--l_ /     ~"-.
              (_/ .  ~(   /'     "~"--,Y   -=b-. _)
               (_/ .  \  :           / l      c"~o \
                \ /    `.    .     .^   \_.-~"~--.  )
                 (_/ .   `  /     /       !       )/
                  / / _.   '.   .':      /        '
                  ~(_/ .   /    _  `  .-<_
                    /_/ . ' .-~" `.  / \  \          ,z=.
                    ~( /   '  :   | K   "-.~-.______//
                      "-,.    l   I/ \_    __{--->._(==.
                       //(     \  <    ~"~"     //
                      /' /\     \  \     ,v=.  ((
                    .^. / /\     "  }__ //===-  `
                   / / ' '  "-.,__ {---(==-
                 .^ '       :  T  ~"   ll
                / .  .  . : | :!        \\
               (_/  /   | | j-"          ~^
                 ~-<_(_.^-~"

Copyright (C) 2020. project violet-server.
Usage: ./hsync [OPTIONS...]
   --help
   -v, --version               Show version information.
   --recover-settings          Recover settings.json
   -s, --start                 Starts hsync [use --start]
```

Very fast metadata synchronizer!

## How to use?

At least 8 GB of memory is required.

```
1. Set your own hitomi.la crawling range
https://github.com/project-violet/violet-server/blob/master/tools/hsync/hsync/Syncronizer.cs#L32
This use exhentai-based ID.
The default of 6,000 means to crawl 157,000 to 163,000 based on 160,000.

2. Set exhentai.org search range
https://github.com/project-violet/violet-server/blob/master/tools/hsync/hsync/Syncronizer.cs#L175
This use exhentai search result page number.
You need to set up how many pages you want to browse. The default is 150 pages.

3. Build
I recommend the debug build, but if you want to release, 
check the batch files in https://github.com/project-violet/violet-server/tree/master/tools/hsync/hsyncc folder.

4. Download base database
Download base database from https://github.com/project-violet/violet-server/releases

5. Run ./hsync.exe

6. Wait for complete
```