# Task Friend
Task Friend runs concurrent tasks.

You specify a file with tasks as input, and Task Friend runs them concurrently and in a friendly manner.

By default, Task Friend will run 10 tasks concurrently, but you can overwrite this behavior as you like.

## Quick start

**task-friend.exe --help**

    Usage: task-friend.exe -i c:\myinput.txt -c 10 [-d] [-s] [-b] [-t milliseconds]

    Runs tasks from an input file (-i) on a number of concurrent tasks (-c) until
    all tasks have been processed

      -i, --input         Required. Input file to be processed. Each line in the
                          file should be a command line input.

      -c, --concurrent    (Default: 10) Number of concurrent tasks (default is 10)

      -t, --timeout       (Default: 60000) Timeout in milliseconds (default is
                          60000)

      -d, --debug         Outputs debug information

      -s, --silent        No command line output

      -b, --break         (Default: False) Break on errors - when enabled Task
                          Friend don't resume task processing if a process returns
                          an error (default is false)

      --help              Display this help screen.

## How it works
### The Input File
You feed Task Friend with an input file where each line is a task that points to an executable and an optional number of parameters - just as you would do in a command prompt. For example:

**myinput.txt**

    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 1
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 2
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 3
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 4
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 5
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 6
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 7
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 8
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 9
    lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 10

In the example above, Task Friend expects lazy-friend.exe to be placed next to Task Friend itself. That is, in the same folder as `task-friend.exe`.

Task Friend will process each task at a time and pass the parameters to the process.

Task Friend accepts fully qualified paths to the file system such as:
**myinput.txt**

    c:\foo\bar\lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 1
    c:\foo\bar\lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 2
    c:\foo\bar\lazy-friend.exe -p1 parameter1 -p2 parameter2 --line 3
    ...

The working directory of the process is always the directory of the task executable. That is, in the example above, the working directory of the process is `c:\foo\bar\`.

### Time-out's and abort's
When you start Task Friend, he will output:

    Doing work for you...
    ---------------------------------------
    Press 'q' to abort all tasks and quit

Task Friend is a real friend, so he is serious about this promise! At any time during the processing you can press 'q' to abort all tasks. This will kill any current process and stop processing of further tasks.

For each task, Task Friend will respect the timeout you specify when calling him. Task Friend defaults to 60 seconds. This timeout applies *per task*. That is, the total execution time of all tasks can be higher than the timeout you have specified (since the timeout is for one task).

### Debugging - What is your new friend doing?
Task Friend is the silent type. Doesn't say much. But by using the `-d` option (d for [d]ebug) Task Friend becomes pretty chatty. You can use this to get to know him better. He'd like that.

But you can also shut him up. If you really want to do this, use the `-s` option (s for [s]ilent -- or if you like -- [s]hut up)

Now, Task Friend doesn't always listen to you. If something really weird happens, he won't shut up, but he'll make sure to tell you about it - you know the type, right? For example, if one of your tasks returns an error.

If errors are important to you as well, use the `-b` (for [b]reak) to break task processing when there's an error in one of your tasks.

Want to store the output in a file? Windows is your friend here. Use `>` to create a new file and `>>` to append to an existing (will create a new if it doesn't exist).

Output from your tasks is only (and always) recorded when debugging is enabled:

    [11:00 PM] Task 10 says: Hello World!

**Example:**
    task-friend.exe -i input.txt > log.txt

### Dates and Times
Task Friend is international - he uses UTC formated date times, so be aware that the hours might not match what you got in the Windows task bar.
