using TestTask;

// checks correct number of arguments
// first argument - Absolute path of the source directory
// second argument - Absolute path of the replica directory
// third argument - Absolute path of the location of log_file
// fourth argument - Synchronization interval (in seconds)
if (args.Length != 4)
    return;

// checks initialization of class Synchronization
Synchronization? sync = null;
try
{
    sync = new(args[0],args[1],args[2]);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message.ToString());
    return;
}

// checks argument for interval
int interval;
try
{
    interval = int.Parse(args[3]);
    if (interval < 1)
        throw new Exception("Interval cannot be less than 1 second.");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message.ToString());
    return;
}

Console.WriteLine("Press any key to terminate program.");

PeriodicSynchronizationStarter starter = new(sync,interval);

// starts periodical process of synchronization
try
{
    starter.Start();
}
catch (Exception ex)
{
   Console.WriteLine(ex.Message.ToString());
}

Console.ReadKey();
// calls to stop PeriodicTimer and waits for it to stop
await starter.Stop();

Console.WriteLine("Exit");