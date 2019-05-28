using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Safester.Controls
{
    public partial class ThreeStateButton : ContentView
    {
        private int selectedOption;
        public int SelectedOption { 
            get{
                return selectedOption;
            }
            set
            {
                if (selectedOption == value)
                    return;
                
                selectedOption = value;
                ChangeView(selectedOption);
            }
        }

        public Action YesAction { get; set; }
        public Action NoAction { get; set; }
        public Action InterAction { get; set; }

        public Object UserData { get; set; }

        public ThreeStateButton()
        {
            InitializeComponent();

            SelectedOption = -1;

            var viewYesTapGestureRecognizer = new TapGestureRecognizer();
            viewYesTapGestureRecognizer.Tapped += (s, e) => {
                ChangeView(0);
            };
            yesView.GestureRecognizers.Add(viewYesTapGestureRecognizer);

            var viewNoTapGestureRecognizer = new TapGestureRecognizer();
            viewNoTapGestureRecognizer.Tapped += (s, e) => {
                ChangeView(1);
            };
            noView.GestureRecognizers.Add(viewNoTapGestureRecognizer);

            var viewInterTapGestureRecognizer = new TapGestureRecognizer();
            viewInterTapGestureRecognizer.Tapped += (s, e) => {
                ChangeView(2);
            };
            interView.GestureRecognizers.Add(viewInterTapGestureRecognizer);
        }

        private void ChangeView(int option)
        {
            SelectedOption = option;
            switch (option)
            {
                case 0:
                    noView.BackgroundColor = Color.Transparent;
                    noView.TextColor = (Color)Application.Current.Resources["Primary"];
                    yesView.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    yesView.TextColor = Color.White;
                    interView.BackgroundColor = Color.Transparent;
                    interView.TextColor = (Color)Application.Current.Resources["Primary"];

                    YesAction?.Invoke();
                    break;
                case 1:
                    yesView.BackgroundColor = Color.Transparent;
                    yesView.TextColor = (Color)Application.Current.Resources["Primary"];
                    noView.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    noView.TextColor = Color.White;
                    interView.BackgroundColor = Color.Transparent;
                    interView.TextColor = (Color)Application.Current.Resources["Primary"];

                    NoAction?.Invoke();
                    break;
                case 2:
                    yesView.BackgroundColor = Color.Transparent;
                    yesView.TextColor = (Color)Application.Current.Resources["Primary"];
                    interView.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                    interView.TextColor = Color.White;
                    noView.BackgroundColor = Color.Transparent;
                    noView.TextColor = (Color)Application.Current.Resources["Primary"];

                    InterAction?.Invoke();
                    break;
            }
        }
    }
}
