using CocosSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using tests;
using AudioTest.UWP;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AudioTest.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            InitializeComponent();

            this.CocosView.ViewCreated += (sender, e) =>
            {
                var gameView = sender as CCGameView;
                var scene = new CCScene(gameView);
                var layer = new CCLayerColor();
                scene.AddLayer(layer);

                var background = new CCSprite("background", null);
                background.Position = layer.VisibleBoundsWorldspace.Center;
                layer.AddChild(background);

                var kurineko = new CCSprite("kurineko", null);
                kurineko.Position = layer.VisibleBoundsWorldspace.Center;
                layer.AddChild(kurineko);

                gameView.RunWithScene(scene);
            };
        }

        void Muau(object sender, RoutedEventArgs e)
        {
            CCAudioEngine.SharedEngine.PlayEffect("meow");
        }
    }//class MainPage
      
}//namespace
