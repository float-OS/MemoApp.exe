using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Threading;

namespace MemoApp;

/// <summary>
/// 메모 프로그램 메인 윈도우
/// </summary>
public partial class MainWindow : Window
{
    private readonly MemoService _memoService;
    private List<Memo> _allMemos = new();
    private List<Memo> _filteredMemos = new();
    private Memo? _currentMemo;
    private bool _isLoading = false;
    private DispatcherTimer? _autoSaveTimer;
    private bool _isFavoriteFilterActive = false;
    private bool _isSortedByDate = true;

    public MainWindow()
    {
        InitializeComponent();
        _memoService = new MemoService();
        
        InitializeUI();
        LoadMemos();
        SetupAutoSave();
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        // 카테고리 목록 초기화
        var categories = new List<string> { "전체", "기본", "업무", "개인", "중요", "아이디어", "할 일", "메모" };
        CategoryComboBox.ItemsSource = categories;
        CategoryComboBox.SelectedIndex = 0;
        
        CategoryEditComboBox.ItemsSource = categories.Skip(1).ToList();
        CategoryEditComboBox.SelectedIndex = 0;

        // 우선순위 목록 초기화
        var priorities = new List<string> { "전체", "일반", "낮음", "보통", "높음" };
        PriorityComboBox.ItemsSource = priorities;
        PriorityComboBox.SelectedIndex = 0;

        var priorityValues = new Dictionary<string, int>
        {
            { "일반", 0 },
            { "낮음", 1 },
            { "보통", 2 },
            { "높음", 3 }
        };
        PriorityEditComboBox.ItemsSource = priorityValues.Keys.ToList();
        PriorityEditComboBox.SelectedIndex = 0;

        // 자동 저장 타이머 설정
        _autoSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3) // 3초마다 자동 저장
        };
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;

        // 저장 위치 표시 업데이트
        UpdateSaveLocationDisplay();
    }

    /// <summary>
    /// 저장 위치 표시 업데이트
    /// </summary>
    private void UpdateSaveLocationDisplay()
    {
        string savePath = _memoService.GetDataDirectory();
        SaveLocationText.Text = $"저장 위치: {savePath}";
    }

    /// <summary>
    /// 메모 로드
    /// </summary>
    private void LoadMemos()
    {
        _isLoading = true;
        _allMemos = _memoService.LoadMemos();
        ApplyFilters();
        _isLoading = false;
        UpdateStatusBar($"총 {_allMemos.Count}개의 메모가 로드되었습니다.");
    }

    /// <summary>
    /// 필터 적용 (최적화된 버전)
    /// </summary>
    private void ApplyFilters()
    {
        if (_allMemos.Count == 0)
        {
            _filteredMemos = new List<Memo>();
            MemoListBox.ItemsSource = _filteredMemos;
            UpdateStatusBar("메모가 없습니다.");
            return;
        }

        var result = new List<Memo>(_allMemos);

        // 즐겨찾기 필터
        if (_isFavoriteFilterActive)
        {
            result = _memoService.FilterFavorites(result);
        }

        // 검색 필터
        string searchText = SearchTextBox.Text;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            result = _memoService.SearchMemos(result, searchText);
        }

        // 카테고리 필터
        string selectedCategory = CategoryComboBox.SelectedItem?.ToString() ?? "전체";
        result = _memoService.FilterByCategory(result, selectedCategory);

        // 우선순위 필터
        string selectedPriority = PriorityComboBox.SelectedItem?.ToString() ?? "전체";
        if (selectedPriority != "전체")
        {
            int priorityValue = selectedPriority switch
            {
                "일반" => 0,
                "낮음" => 1,
                "보통" => 2,
                "높음" => 3,
                _ => -1
            };
            if (priorityValue >= 0)
            {
                result = _memoService.FilterByPriority(result, priorityValue);
            }
        }

        // 정렬
        _filteredMemos = _isSortedByDate 
            ? result.OrderByDescending(m => m.ModifiedDate).ToList()
            : result.OrderBy(m => m.Title).ToList();

        // 목록 업데이트
        MemoListBox.ItemsSource = _filteredMemos;

        UpdateStatusBar($"총 {_allMemos.Count}개의 메모 중 {_filteredMemos.Count}개 표시");
    }

    /// <summary>
    /// 현재 메모 표시
    /// </summary>
    private void DisplayMemo(Memo? memo)
    {
        _isLoading = true;
        _currentMemo = memo;

        if (memo == null)
        {
            TitleTextBox.Text = string.Empty;
            ContentTextBox.Text = string.Empty;
            CategoryEditComboBox.SelectedItem = "기본";
            TagsTextBox.Text = string.Empty;
            FavoriteCheckBox.IsChecked = false;
            PriorityEditComboBox.SelectedItem = "일반";
            DateTextBlock.Text = string.Empty;
        }
        else
        {
            TitleTextBox.Text = memo.Title;
            ContentTextBox.Text = memo.Content;
            CategoryEditComboBox.SelectedItem = memo.Category;
            TagsTextBox.Text = memo.Tags;
            FavoriteCheckBox.IsChecked = memo.IsFavorite;
            
            string priorityText = memo.Priority switch
            {
                1 => "낮음",
                2 => "보통",
                3 => "높음",
                _ => "일반"
            };
            PriorityEditComboBox.SelectedItem = priorityText;
            
            DateTextBlock.Text = $"생성: {memo.CreatedDate:yyyy-MM-dd HH:mm} | 수정: {memo.ModifiedDate:yyyy-MM-dd HH:mm}";
        }

        _isLoading = false;
    }

    /// <summary>
    /// 메모 저장
    /// </summary>
    private void SaveMemo()
    {
        if (_currentMemo == null)
        {
            // 새 메모 생성
            _currentMemo = new Memo
            {
                Title = TitleTextBox.Text.Trim(),
                Content = ContentTextBox.Text,
                Category = CategoryEditComboBox.SelectedItem?.ToString() ?? "기본",
                Tags = TagsTextBox.Text.Trim(),
                IsFavorite = FavoriteCheckBox.IsChecked ?? false,
                Priority = GetPriorityValue(PriorityEditComboBox.SelectedItem?.ToString() ?? "일반")
            };
            _allMemos.Add(_currentMemo);
        }
        else
        {
            // 기존 메모 업데이트
            _currentMemo.Title = TitleTextBox.Text.Trim();
            _currentMemo.Content = ContentTextBox.Text;
            _currentMemo.Category = CategoryEditComboBox.SelectedItem?.ToString() ?? "기본";
            _currentMemo.Tags = TagsTextBox.Text.Trim();
            _currentMemo.IsFavorite = FavoriteCheckBox.IsChecked ?? false;
            _currentMemo.Priority = GetPriorityValue(PriorityEditComboBox.SelectedItem?.ToString() ?? "일반");
            _currentMemo.ModifiedDate = DateTime.Now;
        }

        try
        {
            _memoService.SaveMemos(_allMemos);
            ApplyFilters();
            UpdateStatusBar($"{DateTime.Now:HH:mm:ss} - 저장 완료");
            
            // 목록에서 현재 메모 선택
            MemoListBox.SelectedItem = _currentMemo;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"저장 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 우선순위 값 가져오기
    /// </summary>
    private int GetPriorityValue(string priorityText)
    {
        return priorityText switch
        {
            "낮음" => 1,
            "보통" => 2,
            "높음" => 3,
            _ => 0
        };
    }

    /// <summary>
    /// 자동 저장 설정
    /// </summary>
    private void SetupAutoSave()
    {
        _autoSaveTimer?.Start();
    }

    /// <summary>
    /// 자동 저장 타이머 이벤트 (최적화된 버전)
    /// </summary>
    private void AutoSaveTimer_Tick(object? sender, EventArgs e)
    {
        if (_currentMemo == null || _isLoading || _autoSaveTimer == null)
            return;

        // 변경사항이 있으면 자동 저장
        string title = TitleTextBox.Text.Trim();
        string content = ContentTextBox.Text;
        
        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(content))
        {
            // 변경사항이 실제로 있는지 확인
            if (_currentMemo.Title != title || _currentMemo.Content != content)
            {
                SaveMemo();
            }
        }
    }

    /// <summary>
    /// 키보드 단축키 처리
    /// </summary>
    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Ctrl+S: 저장
        if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            SaveButton_Click(sender, e);
            e.Handled = true;
            return;
        }

        // Ctrl+N: 새 메모
        if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            NewMemoButton_Click(sender, e);
            e.Handled = true;
            return;
        }

        // Delete: 삭제 (메모가 선택된 경우)
        if (e.Key == Key.Delete && _currentMemo != null)
        {
            DeleteButton_Click(sender, e);
            e.Handled = true;
            return;
        }

        // Ctrl+F: 검색창 포커스
        if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
            e.Handled = true;
            return;
        }

        // Escape: 검색창 초기화
        if (e.Key == Key.Escape && SearchTextBox.IsFocused)
        {
            SearchTextBox.Text = string.Empty;
            TitleTextBox.Focus();
            e.Handled = true;
        }
    }

    /// <summary>
    /// 상태바 업데이트
    /// </summary>
    private void UpdateStatusBar(string message)
    {
        StatusTextBlock.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
    }

    // 이벤트 핸들러들

    private void NewMemoButton_Click(object sender, RoutedEventArgs e)
    {
        MemoListBox.SelectedItem = null;
        DisplayMemo(null);
        TitleTextBox.Focus();
        UpdateStatusBar("새 메모 생성");
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveMemo();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "메모 데이터를 저장할 폴더를 선택하세요",
            SelectedPath = _memoService.GetDataDirectory()
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                // 기존 데이터 백업
                var currentMemos = _memoService.LoadMemos();
                
                // 저장 위치 변경
                _memoService.SetDataDirectory(dialog.SelectedPath);
                
                // 데이터 저장
                _memoService.SaveMemos(currentMemos);
                
                // UI 업데이트
                UpdateSaveLocationDisplay();
                LoadMemos();
                
                System.Windows.MessageBox.Show(
                    $"저장 위치가 변경되었습니다.\n새 위치: {dialog.SelectedPath}",
                    "설정 변경",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"저장 위치 변경 실패: {ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentMemo == null)
        {
            System.Windows.MessageBox.Show("내보낼 메모를 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*",
            FileName = $"{_currentMemo.Title}.txt"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                _memoService.ExportMemo(_currentMemo, saveDialog.FileName);
                System.Windows.MessageBox.Show("메모가 성공적으로 내보내졌습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateStatusBar("메모 내보내기 완료");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"내보내기 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentMemo == null)
        {
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"메모 '{_currentMemo.Title}'를 삭제하시겠습니까?",
            "메모 삭제",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _memoService.DeleteMemo(_currentMemo.Id, _allMemos);
            _currentMemo = null;
            DisplayMemo(null);
            ApplyFilters();
            UpdateStatusBar("메모 삭제 완료");
        }
    }

    private void MemoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MemoListBox.SelectedItem is Memo selectedMemo)
        {
            DisplayMemo(selectedMemo);
        }
    }

    private DispatcherTimer? _searchDebounceTimer;

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // 검색 디바운싱으로 성능 최적화 (500ms 지연)
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _searchDebounceTimer.Tick += (s, args) =>
        {
            _searchDebounceTimer?.Stop();
            ApplyFilters();
        };
        _searchDebounceTimer.Start();
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void PriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void FavoriteFilterButton_Click(object sender, RoutedEventArgs e)
    {
        _isFavoriteFilterActive = !_isFavoriteFilterActive;
        FavoriteFilterButton.Content = _isFavoriteFilterActive ? "⭐ 즐겨찾기 (활성)" : "⭐ 즐겨찾기";
        FavoriteFilterButton.Background = _isFavoriteFilterActive 
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 156, 18));
        ApplyFilters();
    }

    private void SortButton_Click(object sender, RoutedEventArgs e)
    {
        _isSortedByDate = !_isSortedByDate;
        SortButton.Content = _isSortedByDate ? "📅 최신순 (활성)" : "📅 최신순";
        SortButton.Background = _isSortedByDate
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
        ApplyFilters();
    }

    private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isLoading && _currentMemo != null)
        {
            _currentMemo.ModifiedDate = DateTime.Now;
        }
    }

    private void ContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isLoading && _currentMemo != null)
        {
            _currentMemo.ModifiedDate = DateTime.Now;
        }
    }

    private void CategoryEditComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isLoading && _currentMemo != null)
        {
            _currentMemo.Category = CategoryEditComboBox.SelectedItem?.ToString() ?? "기본";
            _currentMemo.ModifiedDate = DateTime.Now;
        }
    }

    private void PriorityEditComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isLoading && _currentMemo != null)
        {
            _currentMemo.Priority = GetPriorityValue(PriorityEditComboBox.SelectedItem?.ToString() ?? "일반");
            _currentMemo.ModifiedDate = DateTime.Now;
        }
    }

    private void TagsTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isLoading && _currentMemo != null)
        {
            _currentMemo.Tags = TagsTextBox.Text.Trim();
            _currentMemo.ModifiedDate = DateTime.Now;
        }
    }

    private void FavoriteCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isLoading && _currentMemo != null)
        {
            _currentMemo.IsFavorite = FavoriteCheckBox.IsChecked ?? false;
            _currentMemo.ModifiedDate = DateTime.Now;
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // 창 닫을 때 자동 저장
        if (_currentMemo != null)
        {
            SaveMemo();
        }
    }
}
