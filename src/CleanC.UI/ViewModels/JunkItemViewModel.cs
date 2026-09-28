using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using CleanC.Core.Models;

namespace CleanC.UI.ViewModels
{
    public class JunkItemViewModel : INotifyPropertyChanged
    {
        public JunkCategory Category { get; }

        public JunkItemViewModel(JunkCategory category)
        {
            Category = category;
            _isChecked = category.IsChecked;
            _statusText = "در صف بهینه‌سازی";
        }

        public string Id => Category.Id;
        public string GroupId => Category.GroupId;
        public string Title => Category.Title;
        public string Description => Category.Description;
        public long Bytes => Category.Bytes;
        public string FormattedBytes => Category.FormattedBytes;
        public bool RequiresElevation => Category.RequiresElevation;

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    Category.IsChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SpinnerVisibility));
                    OnPropertyChanged(nameof(GlyphVisibility));
                    OnPropertyChanged(nameof(ItemBorderBrush));
                    OnPropertyChanged(nameof(StatusBadgeBackgroundBrush));
                    OnPropertyChanged(nameof(StatusBadgeForegroundBrush));
                }
            }
        }

        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                if (_isSuccess != value)
                {
                    _isSuccess = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentGlyph));
                    OnPropertyChanged(nameof(GlyphBrush));
                    OnPropertyChanged(nameof(ItemBorderBrush));
                    OnPropertyChanged(nameof(StatusBadgeBackgroundBrush));
                    OnPropertyChanged(nameof(StatusBadgeForegroundBrush));
                }
            }
        }

        private bool _wasEmpty;
        public bool WasEmpty
        {
            get => _wasEmpty;
            set
            {
                if (_wasEmpty != value)
                {
                    _wasEmpty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusBadgeBackgroundBrush));
                    OnPropertyChanged(nameof(StatusBadgeForegroundBrush));
                }
            }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility SpinnerVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GlyphVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;

        public string CurrentGlyph => IsSuccess ? "\uE73E" : Category.IconGlyph;

        private static readonly SolidColorBrush SuccessBrush = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)); // Emerald Green
        private static readonly SolidColorBrush AccentBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
        private static readonly SolidColorBrush MutedBrush = new SolidColorBrush(Color.FromArgb(160, 128, 128, 128));
        private static readonly SolidColorBrush InfoBrush = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
        private static readonly SolidColorBrush NormalBorderBrush = new SolidColorBrush(Color.FromArgb(28, 128, 128, 128));
        private static readonly SolidColorBrush AccentBorderBrush = new SolidColorBrush(Color.FromArgb(90, 0, 120, 215));
        private static readonly SolidColorBrush SuccessBorderBrush = new SolidColorBrush(Color.FromArgb(90, 16, 185, 129));
        private static readonly SolidColorBrush SuccessBgBrush = new SolidColorBrush(Color.FromArgb(32, 16, 185, 129));
        private static readonly SolidColorBrush AccentBgBrush = new SolidColorBrush(Color.FromArgb(28, 0, 120, 215));
        private static readonly SolidColorBrush InfoBgBrush = new SolidColorBrush(Color.FromArgb(24, 100, 149, 237));
        private static readonly SolidColorBrush DefaultPillBgBrush = new SolidColorBrush(Color.FromArgb(18, 128, 128, 128));

        public Brush GlyphBrush => IsSuccess ? SuccessBrush : (IsLoading ? AccentBrush : MutedBrush);
        public Brush ItemBorderBrush => IsSuccess ? SuccessBorderBrush : (IsLoading ? AccentBorderBrush : NormalBorderBrush);
        public Brush StatusBadgeBackgroundBrush => IsSuccess ? (WasEmpty ? InfoBgBrush : SuccessBgBrush) : (IsLoading ? AccentBgBrush : DefaultPillBgBrush);
        public Brush StatusBadgeForegroundBrush => IsSuccess ? (WasEmpty ? InfoBrush : SuccessBrush) : (IsLoading ? AccentBrush : MutedBrush);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
