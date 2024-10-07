using Microsoft.VisualBasic;

namespace TestTask
{
    /// <summary>
    /// Class <c>Synchronization</c> synchnonizes one-way two given directories.
    /// After synchronization replica directory will match source directory.
    /// </summary>
    /// <param name="source">Absolute path of the source directory</param>
    /// <param name="replica">Absolute path of the replica directory</param>
    /// <param name="log_file">Absolute path of the location of log_file</param>
    public class Synchronization(string source, string replica, string log_file)
    {
        /// <summary>
        /// Attribut <c>_source</c> holds DirectoryInfo of source directory
        /// </summary>
        protected readonly DirectoryInfo _source = new(source);
        /// <summary>
        /// Attribut <c>_replica</c> holds DirectoryInfo of replica directory
        /// </summary>
        protected readonly DirectoryInfo _replica = new(replica);
        /// <summary>
        /// Attribut <c>_log</c> holds absolute path of log_file
        /// </summary>
        protected readonly string _log = log_file;
        /// <summary>
        /// Attribut <c>_writer</c> will hold stream to the log_file after method Start is called
        /// </summary>
        protected StreamWriter? _writer;

        /// <summary>
        /// Helper class <c>DirectoryInfoComparer</c> is used for helping create set of directories
        /// For comparison is used only name of directory, because it's used only for subdirectories in one directory
        /// </summary>
        private class DirectoryInfoComparer : IEqualityComparer<DirectoryInfo>
        {
            public bool Equals(DirectoryInfo? x, DirectoryInfo? y)
            {
                if (y == null)
                    return x ==null;
                return x == null ? y == null : x.Name.Equals(y.Name);
            }
            public int GetHashCode(DirectoryInfo obj)
            {
                return obj.Name.GetHashCode();
            }

        }

        /// <summary>
        /// Helper class <c>FileInfoComparer</c> is used for helping create set of files
        /// For comparison is used only name of file, because it's used only for files in one directory
        /// </summary>
        private class FileInfoComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo? x, FileInfo? y)
            {
                if (y == null)
                    return x ==null;
                return x == null ? y == null : x.Name.Equals(y.Name);
            }
            public int GetHashCode(FileInfo obj)
            {
                return obj.Name.GetHashCode();
            }

        }

        /// <summary>
        /// Method <c>Log</c> writes action(create,delete,copy) and name of file or directory in question 
        /// to the console and log file
        /// </summary>
        /// <param name="action">name of completed action (create,delete,copy)</param>
        /// <param name="name">name of file or directory to which the action happened</param>
        private void Log(in string action, in string name)
        {
            _writer?.WriteLine($"{action} - {name}");
            Console.WriteLine($"{action} - {name}");
        }

        /// <summary>
        /// Method <c>Start</c> starts the synchronization process
        /// </summary>
        public void Start()
        {
            Checks();
            using (_writer = new(_log,true))
            {
                _writer.WriteLine(DateAndTime.Now);
                Console.WriteLine(DateAndTime.Now);
                SynchronizeDirectories(_source,_replica);
                _writer.Close();
            }
        }

        /// <summary>
        /// Method <c>Checks</c> checks if path to the source and replica directories exists.
        /// If not, DirectoryNotFoundException is thrown.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private void Checks()
        {
            if (_source.Exists == false)
                throw new DirectoryNotFoundException("Source directory not found!");
            if (_replica.Exists == false)
                throw new DirectoryNotFoundException("Replica directory not found!");
        }

        /// <summary>
        /// Method <c>CheckSameNameFiles</c> in for cycle checks if files in <c>Check_equality</c> with the same name
        /// have the same contents in both directories(Source,Replica).
        /// </summary>
        /// <param name="Check_equality">contains files that have the same name in both directories(Source,Replica)</param>
        /// <param name="Source">Directory found in _source directory</param>
        /// <param name="Replica">Directory found in _replica directory</param>
        private void CheckSameNameFiles(IEnumerable<FileInfo> Check_equality, DirectoryInfo Source, DirectoryInfo Replica)
        {
            foreach (FileInfo file in Check_equality)
            {
                try
                {
                    FileInfo source_file = new(Source.FullName + Path.DirectorySeparatorChar + file.Name);
                    FileInfo replica_file = new(Replica.FullName + Path.DirectorySeparatorChar + file.Name);

                    // compares contents of two files
                    // if method Compare returns false, contents are not the same and we have to copy the source_file
                    if (!FileComparer.Compare(source_file, replica_file))
                    {
                        // copies source_file to replica_file 
                        file.CopyTo(replica_file.FullName,true);
                        Log("Copy of file",$"from {file.FullName} to {replica_file.FullName}");
                    }
                }
                catch
                {
                    Log("Unsuccessful Copy of file", $"from {file.FullName}");
                }
            }
        }

        /// <summary>
        /// Method <c>SynchronizeFiles</c> compares names of files in directories Source and Replica.
        /// If unique name is found in Replica directory, file is deleted.
        /// If unique name is found in Source directory, file is created in Replica directory.
        /// If name is found in both directories, files are checked in method <c>CheckSameNameFiles</c>.
        /// </summary>
        /// <param name="Source">Current working directory found in _source directory</param>
        /// <param name="Replica">Current working directory found in _replica directory</param>
        private void SynchronizeFiles(DirectoryInfo Source, DirectoryInfo Replica)
        {
            // get arrays of files in given directory
            FileInfo[] Source_list = Source.GetFiles();
            FileInfo[] Replica_list = Replica.GetFiles();

            // creates sets of files depending on what action needs to be done with current file
            IEnumerable<FileInfo> Delete_in_Replica = Replica_list.Except(Source_list, new FileInfoComparer());
            IEnumerable<FileInfo> Copy_to_Replica = Source_list.Except(Replica_list, new FileInfoComparer());
            IEnumerable<FileInfo> Check_equality = Source_list.Intersect(Replica_list, new FileInfoComparer());

            // deletes files in Replica directory
            Delete_in_Replica.ToList().ForEach(R => 
            {
                try{
                    R.Delete();
                    Log("Delete file",R.FullName);
                }
                catch
                {
                    Log("Unsuccessful Delete of file", R.FullName);
                }
            });
            
            // creates files in Replica directory
            string full_name_replica = Replica.FullName;
            Copy_to_Replica.ToList().ForEach(R => 
            {
                string new_file = full_name_replica + Path.DirectorySeparatorChar + R.Name;
                try{
                    R.CopyTo(new_file);
                    Log("Create file",new_file);
                }
                catch
                {
                    Log("Unsuccessful Create of file", new_file);
                }

            });

            // checks contents of files with the same name 
            CheckSameNameFiles(Check_equality,Source,Replica);
        }

        /// <summary>
        /// Method <c>SynchronizeDirectories</c> compares names of subdirectories in directories Source and Replica.
        /// If unique name is found in Replica directory, subdirectory is deleted.
        /// If unique name is found in Source directory, subdirectory is created in Replica directory.
        /// For every subdirectory is process repeated
        /// At the end of method, method <c>SynchronizeFiles</c> is called to synchronize files in the Source and Replica directory
        /// </summary>
        /// <param name="Source">Current working directory found in _source directory</param>
        /// <param name="Replica">Current working directory found in _replica directory</param>
        private void SynchronizeDirectories(DirectoryInfo Source, DirectoryInfo Replica)
        {
            // get arrays of subdirectories in given directory
            DirectoryInfo[] Source_list = Source.GetDirectories();
            DirectoryInfo[] Replica_list = Replica.GetDirectories();

            // creates sets of subdirectories depending on what action needs to be done with it
            IEnumerable<DirectoryInfo> Delete_in_Replica = Replica_list.Except(Source_list, new DirectoryInfoComparer());
            IEnumerable<DirectoryInfo> Create_in_Replica = Source_list.Except(Replica_list, new DirectoryInfoComparer());

            // deletes subdirectories in Replica directory
            Delete_in_Replica.ToList().ForEach(R => {R.Delete(true); Log("Delete Directory",R.FullName);});

            string full_name_replica = Replica.FullName;
            // creates subdirectories in Replica directory
            Create_in_Replica.ToList().ForEach(R => {string new_dir = full_name_replica + Path.DirectorySeparatorChar + R.Name;
                                                        Directory.CreateDirectory(new_dir);
                                                        Log("Create Directory",new_dir);});


            Replica_list = Replica.GetDirectories();
            // synchronizes subdirectories one after another
            for (int i = 0; i < Source_list.Length; i++)
            {
                SynchronizeDirectories(Source_list[i],Replica_list[i]);
            }

            // synchronizes files in current Source and Replica directories.
            SynchronizeFiles(Source,Replica);

        }
    }
}