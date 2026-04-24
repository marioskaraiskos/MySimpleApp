using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace MySimpleApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AddNewTextField(); // Start with the first one
        }

        private void AddNewTextField()
        {
            // Requirement: Max 5 times
            if (DynamicContainer.Children.Count >= 5) 
            {
                LimitWarning.Visibility = Visibility.Visible;
                return;
            }

            // Create a wrapper for the TextBox and Delete button
            var row = new Grid { Margin = new Thickness(0, 5, 0, 5) };
// This part is crucial for the button to show up to the right!
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // TextBox takes rest
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Button takes only what it needs

            var input = new TextBox
            {
                Style = (Style)Application.Current.Resources["MyPrettyField"],
                Tag = row // Store row reference for easy removal
            };

            input.PreviewTextInput += (s, e) => {
    // Regex: Only allow a-z, A-Z, and 0-9
    // ^ means start, $ means end, [a-zA-Z0-9] is our allowed set
    Regex regex = new Regex("^[a-zA-Z0-9]+$");
    
    // If the new text does NOT match the regex, mark as Handled (blocks input)
    e.Handled = !regex.IsMatch(e.Text);
};

            // Event 1: Keyboard Logic (Enter, Space, Down, Backspace)
            input.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Down) {
                    if (string.IsNullOrWhiteSpace(input.Text)) 
                    {
                    // Optional: Give visual feedback that it's empty
                        input.BorderBrush = Brushes.Red; 
                        e.Handled = true; // Block the action
                        return; 
                    }

                    // If we got here, there is text! Reset border and proceed.
                    input.BorderBrush = Brushes.Black;
                    int rowIndex = DynamicContainer.Children.IndexOf(row);
                    if (rowIndex > 0) 
                    {
                        AddDeleteButton(row);
                    }

                    AddNewTextField();
                    e.Handled = true;
                }
                
                // Backspace delete: if at index 0 and not the only field
                if (e.Key == Key.Back && input.CaretIndex == 0 && DynamicContainer.Children.Count > 1) {
                    DynamicContainer.Children.Remove(row);
                    LimitWarning.Visibility = Visibility.Collapsed;
                    // Focus the previous field
                    var lastRow = DynamicContainer.Children.OfType<Grid>().LastOrDefault();
                    (lastRow?.Children[0] as TextBox)?.Focus();
                }
            };

            // Event 2: Trigger word check ("pressed" / "submitted")
            input.TextChanged += (s, e) => {
                string val = input.Text.ToLower();
                if (val.Contains("pressed") || val.Contains("submitted")) {
                    int rowIndex = DynamicContainer.Children.IndexOf(row);
                    if (rowIndex == 0)
                    {
                        return;
                    }
                    AddDeleteButton(row);
                }
            };

            Grid.SetColumn(input, 0);
            row.Children.Add(input);
            DynamicContainer.Children.Add(row);
            input.Focus();
        }

        private void AddDeleteButton(Grid row)
        {
            // Don't add if it already has a button
            if (row.Children.Count > 1) return;

            var btn = new Button
            {
                Content = "X",
                Width = 40,
                Height = 40,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 10, 0, 0),
                Background = Brushes.Black,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0), // Clean look
                Cursor = Cursors.Hand
            };

            btn.Click += (s, e) => {
                DynamicContainer.Children.Remove(row);
                LimitWarning.Visibility = Visibility.Collapsed;
            };

            Grid.SetColumn(btn, 1);
            row.Children.Add(btn);
        }
    }
}