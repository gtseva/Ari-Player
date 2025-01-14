using System.IO;
using System.Text.Json;
using System.Windows;

namespace Ari_Player
{
    public partial class Settings : Window
    {
        // Declaración de Variable de Carpeta que Almacena la carpeta de la música
        public string musicFolder = string.Empty;
        // Declaración de Variable que genera y se refiere al json que almacena el cache de la música
        public const string CacheFileName = "music_cache.json";
        // Variable de tipo lista string que genera la música en cache
        public List<string> cachedMusicList = new List<string>();

        public Settings()
        {
            InitializeComponent();
        }

        private void OpenFolder_click(object sender, RoutedEventArgs e)
        {
            // Crear el diálogo de selección de carpetas
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            // Mostrar el diálogo y obtener el resultado
            var result = dialog.ShowDialog();

            // Comprobar si el usuario seleccionó una carpeta
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Guardar la ruta de la carpeta seleccionada en la variable
                musicFolder = dialog.SelectedPath;
                cachedMusicList = new List<string>(Directory.GetFiles(musicFolder, "*.mp3"));
                SaveCache();
                // Mostrar la carpeta seleccionada (opcional, para verificar)
                System.Windows.MessageBox.Show($"Carpeta seleccionada: {musicFolder}");
            }
        }

        private void SaveCache()
        {
            try
            {
                string json = JsonSerializer.Serialize(cachedMusicList);
                File.WriteAllText(CacheFileName, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al guardar la caché: {ex.Message}");
            }
        }

        /// <summary>
        /// Método para cargar la lista de canciones desde la caché (archivo JSON)
        /// </summary>
        public void LoadCache()
        {
            try
            {
                if (File.Exists(CacheFileName))
                {
                    string json = File.ReadAllText(CacheFileName);
                    cachedMusicList = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                }
                else
                {
                    System.Windows.MessageBox.Show("No se encontró el archivo de caché. Por favor, selecciona una carpeta de música.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al cargar la caché: {ex.Message}");
            }
        }

        private void ToPlayer_button(object sender, RoutedEventArgs e)
        {
            Player playerWindow = new Player();
            playerWindow.Show();
            this.Close();
        }
    }
}
