// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ChewyMoon">
//   Copyright (C) 2015 ChewyMoon
//   
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace FolderOrganizer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     The program.
    /// </summary>
    internal class Program
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Moves the directory.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static void MoveDirectory(string source, string target)
        {
            var p = new ProcessStartInfo("cmd", $"/c move \"{source}\" \"{target}\"")
                        {
                           WindowStyle = ProcessWindowStyle.Hidden 
                        };
            Process.Start(p);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The main.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            Console.WriteLine(
                "FolderOrganizer version {0} | Created by ChewyMoon.\n", 
                Assembly.GetExecutingAssembly().GetName().Version);
            Console.Write(
                "FolderOrganizer will now organize the folder: \n{0}\n\nContinue? (y/n): ", 
                Environment.CurrentDirectory);

            var input = Console.ReadLine();

            if (input?.ToLower() != "y")
            {
                return;
            }

            Console.WriteLine();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // A-E, F-J, K-O, P-T, and U-Z
            var groups = new List<AlphabeticalGrouping>
                             {
                                 new AlphabeticalGrouping("A-E"), new AlphabeticalGrouping("F-J"), 
                                 new AlphabeticalGrouping("K-O"), new AlphabeticalGrouping("P-T"), 
                                 new AlphabeticalGrouping("U-Z")
                             };

            // Get the name of the folders in the current directory, but don't include the group directories if they're already created.
            var folderNames = Directory.GetDirectories(Directory.GetCurrentDirectory()).Where(
                x =>
                    {
                        var fileName = Path.GetFileName(x);
                        return fileName != null
                               && (groups.All(y => y.Representation != fileName) && !fileName.Equals("Other"));
                    });
            var groupDirectories = new Dictionary<AlphabeticalGrouping, string>();

            // Create the directories and then save the directory location and representation to a dictionary.
            foreach (var group in groups.Select(x => x.Representation))
            {
                var dir = Directory.CreateDirectory(group);
                groupDirectories.Add(groups.First(x => x.Representation.Equals(group)), dir.FullName);
            }

            foreach (var folder in folderNames)
            {
                try
                {
                    // Get the display name of the folder
                    var folderName = Path.GetFileName(folder);

                    if (folderName == null)
                    {
                        Console.WriteLine("Folder name was null. Skipping {0}.", folder);
                        continue;
                    }

                    Console.WriteLine("Processing {0}", folder);

                    // Get the group that the folder should be in.
                    var group =
                        groups.FirstOrDefault(
                            x => folderName.ToUpper()[0] >= x.StartLetter && folderName.ToUpper()[0] <= x.EndLetter);

                    if (group != null)
                    {
                        MoveDirectory(folder, groupDirectories[group]);
                    }
                    else
                    {
                        // Move any other directory that is not A-Z to a folder called "Other"
                        if (!Directory.Exists("Other"))
                        {
                            Directory.CreateDirectory("Other");
                        }

                        var otherDir = Directory.GetDirectories(Directory.GetCurrentDirectory()).First(
                            x =>
                                {
                                    var fileName = Path.GetFileName(x);
                                    return fileName != null && fileName.Equals("Other");
                                });
                        MoveDirectory(folder, otherDir);
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error moving {0}! Exception: \n{1}", folder, e);
                    Console.ResetColor();
                }
            }

            stopwatch.Stop();

            Console.Write(
                "Done! (Completed in {0}s) Press any key to exit.", 
                Math.Round(stopwatch.ElapsedMilliseconds / 1000d, 2));
            Console.ReadKey(true);
        }

        #endregion
    }

    /// <summary>
    ///     Represents a grouping of the alphabet.
    /// </summary>
    internal class AlphabeticalGrouping
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AlphabeticalGrouping" /> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public AlphabeticalGrouping(string representation)
        {
            this.Representation = representation;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the ending letter.
        /// </summary>
        /// <value>
        ///     The ending letter.
        /// </value>
        public char EndLetter => this.Representation.Split('-').Last().ToCharArray().First();

        /// <summary>
        ///     Gets or sets the representation.
        /// </summary>
        /// <value>
        ///     The representation.
        /// </value>
        public string Representation { get; set; }

        /// <summary>
        ///     Gets the starting letter.
        /// </summary>
        /// <value>
        ///     The starting letter.
        /// </value>
        public char StartLetter => this.Representation.Split('-').First().ToCharArray().First();

        #endregion
    }
}
