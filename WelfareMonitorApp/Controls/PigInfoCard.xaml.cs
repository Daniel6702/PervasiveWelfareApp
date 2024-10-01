using Microsoft.Maui.Controls;
using WelfareMonitorApp.Models;

namespace WelfareMonitorApp.Controls
{
    public partial class PigInfoCard : ContentView
    {
        public PigInfoCard()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty PigProperty =
            BindableProperty.Create(nameof(Pig), typeof(Pig), typeof(PigInfoCard), default(Pig), propertyChanged: OnPigChanged);

        public Pig Pig
        {
            get => (Pig)GetValue(PigProperty);
            set => SetValue(PigProperty, value);
        }

        public static readonly BindableProperty StatusColorProperty =
            BindableProperty.Create(nameof(StatusColor), typeof(Color), typeof(PigInfoCard), Colors.Green);

        public Color StatusColor
        {
            get => (Color)GetValue(StatusColorProperty);
            set => SetValue(StatusColorProperty, value);
        }

        private static void OnPigChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var card = (PigInfoCard)bindable;
            if (newValue is Pig pig)
            {
                // Update the UI with new pig information
                card.BindingContext = pig;
                
                // Update the background color based on current status
                card.UpdateBackgroundColor(pig.Status);
            }
        }

        private void UpdateBackgroundColor(string status)
        {
            // Set the StatusColor based on the pig's status
            switch (status.ToLower())
            {
                case "healthy":
                    StatusColor = Colors.Green; // Healthy
                    break;
                case "caution":
                    StatusColor = Colors.Yellow; // Caution
                    break;
                case "alert":
                    StatusColor = Colors.Red; // Alert
                    break;
                default:
                    StatusColor = Colors.Gray; // Default case if status is unknown
                    break;
            }

            // Update the Frame's background color
            this.BackgroundColor = StatusColor; // This sets the background color of the entire control
        }
    }
}
