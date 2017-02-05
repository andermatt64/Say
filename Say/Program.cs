using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

using CommandLine;
using CommandLine.Text;

namespace Say
{
    class Options
    {
        [Option('o', "output", HelpText = "Audio file to save synthesized speech")]
        public string OutputFile { get; set; }

        [Option('i', "input", HelpText = "Path to a file (SSML capable)")]
        public string InputFile { get; set; }

        [ValueOption(0)]
        public string Text { get; set; }

        [Option('l', "list-voices", DefaultValue = false, HelpText = "List available voices (use SSML to dictate voice/age/gender)")]
        public bool ListVoices { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        static void ListVoices()
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                Console.WriteLine("Installed Voices:");

                foreach (InstalledVoice voice in synth.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;

                    Console.WriteLine("  + Name : " + info.Name);
                    Console.WriteLine("     - ID      : " + info.Id);
                    Console.WriteLine("     - Culture : " + info.Culture);
                    Console.WriteLine("     - Age     : " + info.Age);
                    Console.WriteLine("     - Gender  : " + info.Gender);
                    Console.WriteLine("     - Enabled : " + voice.Enabled);

                    string AudioFormats = "";
                    foreach (SpeechAudioFormatInfo fmt in info.SupportedAudioFormats)
                    {
                        AudioFormats += String.Format("       * {0}\n",
                            fmt.EncodingFormat.ToString());
                    }

                    if (info.SupportedAudioFormats.Count != 0)
                    {
                        Console.WriteLine("     - Audio Formats");
                        Console.WriteLine(AudioFormats);
                    } 
                    else
                    {
                        Console.WriteLine("     - No supported audio formats");
                    }

                    Console.WriteLine();
                }
            }
        }

        static void SayText(bool is_ssml, string text, string path)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            if (path != null)
            {
                synth.SetOutputToWaveFile(path);
            }
            else
            {
                synth.SetOutputToDefaultAudioDevice();
            }

            if (is_ssml)
            {
                try
                {
                    synth.SpeakSsml(text);
                } 
                catch (FormatException e)
                {
                    Console.WriteLine("Bad SSML! " + e.Message);
                }
            }
            else
            {
                synth.Speak(text);
            }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.ListVoices)
                {
                    ListVoices();
                }
                else
                {
                    if (options.Text == null && options.InputFile != null)
                    {
                        if (File.Exists(options.InputFile))
                        {
                            string text = File.ReadAllText(options.InputFile);

                            SayText(true, text, options.OutputFile);
                        }
                        else
                        {
                            Console.WriteLine("File does not exist: " + options.InputFile);
                        }
                    }
                    else if (options.Text != null && options.InputFile == null)
                    {
                        SayText(false, options.Text, options.OutputFile);
                    }
                    else
                    {
                        Console.WriteLine("Must provide text OR an input file!\n");
                    }
                }
            }
        }
    }
}
