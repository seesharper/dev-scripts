using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;

public static class Command
{
    public static CommandResult Capture(string commandPath, string arguments, string workingDirectory = null)
    {
        var startInformation =  CreateProcessStartInfo(commandPath, arguments, workingDirectory);
        var process = CreateProcess(startInformation);
        process.Start();
        process.WaitForExit();
        return new CommandResult(process.ExitCode, process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
    }

    public static void ShellExecute(string command, string args = null)
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo(command,args);
        processStartInfo.UseShellExecute = true;
        var process = Process.Start(processStartInfo);
        process.WaitForExit();
    }

    public static int Execute(string commandPath, string arguments, string workingDirectory = null,int success = 0)
    {
        var startInformation =  CreateProcessStartInfo(commandPath, arguments, workingDirectory);
        var process = CreateProcess(startInformation);
        process.OutputDataReceived += (o,a) => WriteStandardOut(a);
        process.ErrorDataReceived += (o,a) => WriteStandardError(a);
        void WriteStandardOut(DataReceivedEventArgs args)
        {
            if (args.Data != null)
            {
                Out.WriteLine(args.Data);
            }
        }

        void WriteStandardError(DataReceivedEventArgs args)
        {
            if (args.Data != null)
            {
                Out.WriteLine(args.Data);
            }
        }

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != success)
        {
            throw new InvalidOperationException($"The command {commandPath} {arguments} failed.");
        }

        return process.ExitCode;
    }
    private static ProcessStartInfo CreateProcessStartInfo(string commandPath, string arguments, string workingDirectory)
    {
        var startInformation = new ProcessStartInfo($"{commandPath}");
        startInformation.CreateNoWindow = true;
        startInformation.Arguments =  arguments;
        startInformation.RedirectStandardOutput = true;
        startInformation.RedirectStandardError = true;
        startInformation.UseShellExecute = false;
        startInformation.WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory;
        return startInformation;
    }

    private static void RunAndWait(Process process)
    {
        process.Start();
        process.WaitForExit();
    }
    private static Process CreateProcess(ProcessStartInfo startInformation)
    {
        var process = new Process();
        process.StartInfo = startInformation;
        return process;
    }
}

public class CommandResult
{
    public CommandResult(int exitCode, string standardOut, string standardError)
    {
        ExitCode = exitCode;
        StandardOut = standardOut;
        StandardError = standardError;
    }
    public string StandardOut { get; }
    public string StandardError { get; }
    public int ExitCode { get; }

    public CommandResult Dump()
    {
        Out.Write(StandardOut);
        Error.Write(StandardError);
        return this;
    }

    public CommandResult EnsureSuccessfulExitCode(int success = 0)
    {
        if (ExitCode != success)
        {
            throw new InvalidOperationException(StandardError);
        }
        return this;
    }
}