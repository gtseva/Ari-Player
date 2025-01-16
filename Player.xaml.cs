using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NAudio.Wave;

namespace AriPlayer
{
    public partial class Player : Window
    {
        private List<Song> songCache = new List<Song>();
        private DispatcherTimer timer;
        private IWavePlayer waveOut;
        private AudioFileReader audioFileReader;
        private const string configFilePath = "config.json"; // Ruta del archivo JSON de configuración

        public Player()
        {
            InitializeComponent();
            InitializeTimer();
            waveOut = new WaveOutEvent();
            waveOut.PlaybackStopped += (s, a) => OnPlaybackStopped();
            SongListView.MouseDoubleClick += SongListView_MouseDoubleClick;
            Volumen.ValueChanged += Volumen_ValueChanged;

            // Cargar la carpeta guardada al inicio
            string lastFolderPath = GetLastFolderPath();
            if (!string.IsNullOrEmpty(lastFolderPath))
            {
                try
                {
                    LoadSongsFromFolder(lastFolderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading songs from saved folder: {ex.Message}");
                }
            }
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Opcional: Actualizar controles de reproducción si es necesario
        }

        private async void LoadFolder_Click(object sender, RoutedEventArgs e)
        {
            // Usar FolderBrowserDialog en un hilo separado para evitar el bloqueo de la UI
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    try
                    {
                        // Guardar la ruta de la carpeta seleccionada
                        SaveLastFolderPath(folderPath);

                        // Llamar a LoadSongsFromFolder en un hilo de fondo
                        await Task.Run(() => LoadSongsFromFolder(folderPath));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading songs: {ex.Message}");
                    }
                }
            }
        }

        private void LoadSongsFromFolder(string folderPath)
        {
            var audioFiles = Directory.GetFiles(folderPath, "*.mp3", SearchOption.AllDirectories);

            if (audioFiles.Length == 0)
            {
                MessageBox.Show("No audio files found in the selected folder.");
                return;
            }

            songCache.Clear();

            foreach (var file in audioFiles)
            {
                var song = new Song
                {
                    FilePath = file,
                    Title = Path.GetFileNameWithoutExtension(file),
                    Artist = "Unknown Artist"
                };
                songCache.Add(song);
            }

            // Usar Dispatcher para actualizar la UI en el hilo principal
            Dispatcher.Invoke(() =>
            {
                SongListView.ItemsSource = songCache;
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SongListView.SelectedItem is Song selectedSong)
            {
                PlaySong(selectedSong);
            }
            else
            {
                MessageBox.Show("Please select a song to play.");
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopPlayback();
        }

        private void NextTrack_Click(object sender, RoutedEventArgs e)
        {
            if (songCache.Count > 0)
            {
                int nextIndex = (SongListView.SelectedIndex + 1) % songCache.Count;
                SongListView.SelectedIndex = nextIndex;
                PlaySong(songCache[nextIndex]);
            }
        }

        private void PreviousTrack_Click(object sender, RoutedEventArgs e)
        {
            if (songCache.Count > 0)
            {
                int prevIndex = (SongListView.SelectedIndex - 1 + songCache.Count) % songCache.Count;
                SongListView.SelectedIndex = prevIndex;
                PlaySong(songCache[prevIndex]);
            }
        }

        private void SongListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SongListView.SelectedItem is Song selectedSong)
            {
                PlaySong(selectedSong);
            }
        }

        private void PlaySong(Song song)
        {
            try
            {
                StopPlayback();

                audioFileReader = new AudioFileReader(song.FilePath)
                {
                    Volume = (float)Volumen.Value / 100f // Configura el volumen inicial según el slider
                };
                waveOut.Init(audioFileReader);
                waveOut.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing file: {ex.Message}");
            }
        }

        private void StopPlayback()
        {
            waveOut.Stop();
            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
        }

        private void OnPlaybackStopped()
        {
            // Playback stops naturally; no automatic track progression a menos que se desee explícitamente
        }

        private void Volumen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (audioFileReader != null)
            {
                // Ajustar el volumen del archivo de audio
                audioFileReader.Volume = (float)e.NewValue / 100f;
            }
        }

        // Clase para manejar la configuración
        private void SaveLastFolderPath(string path)
        {
            var config = new Config { LastFolderPath = path };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configFilePath, json);
        }

        private string GetLastFolderPath()
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                var config = JsonSerializer.Deserialize<Config>(json);
                return config?.LastFolderPath;
            }
            return string.Empty;
        }

        // Clase para representar la configuración
        public class Config
        {
            public string LastFolderPath { get; set; }
        }

        public class Song
        {
            public string FilePath { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
        }
    }
}
