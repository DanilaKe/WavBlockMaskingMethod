using System;

namespace WavBlockMaskingMethod
{
    class EntryPoint
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Gets command line args. "filename of WAV file" "option(hide/extract)" "text"</param>
        static void Main(string[] args)
        {
            var (filePath, actionType, expectedText) = ParseCommandLineArgs(args);
            var method = new BlockMaskingMethod();

            switch (actionType)
            {
                case Action.Hide:
                    method.HideMessage(filePath, expectedText);
                    break;
                case Action.Extract:
                    Console.WriteLine(method.ExtractMessage(filePath));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static (string, Action, string) ParseCommandLineArgs(string[] args)
        {
            var actionType = (Action)Enum.Parse(typeof(Action), args[1], true);
            switch (actionType)
            {
                case Action.Hide:
                    if (args.Length != 3)
                        throw new ArgumentException();
                    return (args[0], actionType, args[2]);
                case Action.Extract:
                    if (args.Length != 2)
                        throw new ArgumentException();
                    return (args[0], actionType, string.Empty);
                default:
                    throw new ArgumentException();
            }
        }

        enum Action
        {
            Hide,
            Extract
        }
    }
}
