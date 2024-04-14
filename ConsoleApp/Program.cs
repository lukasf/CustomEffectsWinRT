using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WinRTLibrary;
using Windows.Foundation.Collections;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Video Effect Sample");
            RenderMediaCompositionWithEffects().Wait();
        }

        private static async Task RenderMediaCompositionWithEffects()
        {
            var composition = new MediaComposition();

            // Add clip
            var clip = MediaClip.CreateFromColor(Color.FromArgb(255, 0, 0, 0), TimeSpan.FromSeconds(5));
            composition.Clips.Add(clip);

            // Add effect
            var blurVideoEffectDefinition = new VideoEffectDefinition(typeof(BlurVideoEffect).FullName);
            clip.VideoEffectDefinitions.Add(blurVideoEffectDefinition);

            //var fadeVideoEffectDefinition = new VideoEffectDefinition(typeof(FadeVideoEffect).FullName);
            //clip.VideoEffectDefinitions.Add(fadeVideoEffectDefinition);

            //var echoAudioEffectDefinition = new AudioEffectDefinition(typeof(EchoAudioEffect).FullName);
            //clip.AudioEffectDefinitions.Add(echoAudioEffectDefinition);

            // Render
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Join(folder, "output_video.mp4");
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            var outputFile = await StorageFile.GetFileFromPathAsync(path);
            var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
            await composition.RenderToFileAsync(outputFile, MediaTrimmingPreference.Precise, profile);

            Console.WriteLine($"{path}");
            Console.WriteLine("Done!");
        }
    }
}