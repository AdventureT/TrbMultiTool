using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrbMultiTool;

namespace TrbMultiTool
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public MainWindow()
		{
			InitializeComponent();
			AddGamesToComboBox();
		}


		private void AddGamesToComboBox()
		{
			ChooseGameComboBox.Items.Add(Game.Barnyard);
			ChooseGameComboBox.Items.Add(Game.NicktoonsUnite);
			ChooseGameComboBox.Items.Add(Game.NicktoonsBattleForVolcanoIsland);
			ChooseGameComboBox.Items.Add(Game.NicktoonsAttackOfTheToybots);
			ChooseGameComboBox.Items.Add(Game.ElTigre);
			ChooseGameComboBox.Items.Add(Game.MarvelSuperHeroSquad);
			ChooseGameComboBox.Items.Add(Game.DeBlob);
			ChooseGameComboBox.SelectedIndex = 0;
		}

		private void ChooseGameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((Game)ChooseGameComboBox.SelectedIndex == Game.Barnyard) ImageBox.Source = new BitmapImage(new Uri("Resources/barnyard.png", UriKind.Relative));
			if ((Game)ChooseGameComboBox.SelectedIndex == Game.NicktoonsUnite) ImageBox.Source = new BitmapImage(new Uri("Resources/nicktoonsUnite.jpg", UriKind.Relative));
		}

		private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Filter = "TRB Files (*.trb)|*.trb|TTL files (*.ttl)|*.ttl",
				DefaultExt = ".trb|.ttl",
				Title = "Select a trb/ttl file"
			};

			if ((bool)openFileDialog.ShowDialog())
			{
				new Trb(openFileDialog.FileName);
			}
		}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
			Environment.Exit(0);
        }
    }
}
