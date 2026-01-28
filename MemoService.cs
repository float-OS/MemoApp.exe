using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MemoApp;

/// <summary>
/// 메모 저장/로드 서비스
/// </summary>
public class MemoService
{
    private string _dataDirectory;
    private string _memosFilePath;

    public MemoService()
    {
        // EXE 파일과 같은 폴더에 저장 (single-file 배포 지원)
        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _dataDirectory = exeDirectory;
        _memosFilePath = Path.Combine(_dataDirectory, "memos.json");

        // 디렉토리가 없으면 생성
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    /// <summary>
    /// 저장 경로 변경
    /// </summary>
    public void SetDataDirectory(string directory)
    {
        _dataDirectory = directory;
        _memosFilePath = Path.Combine(_dataDirectory, "memos.json");
        
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    /// <summary>
    /// 현재 저장 경로 가져오기
    /// </summary>
    public string GetDataDirectory()
    {
        return _dataDirectory;
    }

    /// <summary>
    /// 모든 메모 로드
    /// </summary>
    public List<Memo> LoadMemos()
    {
        if (!File.Exists(_memosFilePath))
        {
            return new List<Memo>();
        }

        try
        {
            string json = File.ReadAllText(_memosFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Memo>();
            }

            var memos = JsonSerializer.Deserialize<List<Memo>>(json);
            return memos ?? new List<Memo>();
        }
        catch
        {
            return new List<Memo>();
        }
    }

    /// <summary>
    /// 메모 저장
    /// </summary>
    public void SaveMemos(List<Memo> memos)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText(_memosFilePath, JsonSerializer.Serialize(memos, options));
        }
        catch (Exception ex)
        {
            throw new Exception($"메모 저장 실패: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 메모 삭제
    /// </summary>
    public void DeleteMemo(string id, List<Memo> memos)
    {
        var memo = memos.FirstOrDefault(m => m.Id == id);
        if (memo != null)
        {
            memos.Remove(memo);
            SaveMemos(memos);
        }
    }

    /// <summary>
    /// 메모 검색 (최적화된 버전)
    /// </summary>
    public List<Memo> SearchMemos(List<Memo> memos, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText) || memos.Count == 0)
        {
            return memos;
        }

        string lowerSearch = searchText.ToLower();
        var results = new List<Memo>(memos.Count);
        
        foreach (var memo in memos)
        {
            if (memo.Title.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase) ||
                memo.Content.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase) ||
                memo.Category.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase) ||
                memo.Tags.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(memo);
            }
        }
        
        return results;
    }

    /// <summary>
    /// 카테고리별 필터링
    /// </summary>
    public List<Memo> FilterByCategory(List<Memo> memos, string category)
    {
        if (string.IsNullOrWhiteSpace(category) || category == "전체")
        {
            return memos;
        }

        return memos.Where(m => m.Category == category).ToList();
    }

    /// <summary>
    /// 즐겨찾기 필터링
    /// </summary>
    public List<Memo> FilterFavorites(List<Memo> memos)
    {
        return memos.Where(m => m.IsFavorite).ToList();
    }

    /// <summary>
    /// 우선순위별 필터링
    /// </summary>
    public List<Memo> FilterByPriority(List<Memo> memos, int priority)
    {
        return memos.Where(m => m.Priority == priority).ToList();
    }

    /// <summary>
    /// 메모 내보내기 (텍스트 파일)
    /// </summary>
    public void ExportMemo(Memo memo, string filePath)
    {
        string content = $"제목: {memo.Title}\n" +
                        $"카테고리: {memo.Category}\n" +
                        $"태그: {memo.Tags}\n" +
                        $"생성일: {memo.CreatedDate:yyyy-MM-dd HH:mm:ss}\n" +
                        $"수정일: {memo.ModifiedDate:yyyy-MM-dd HH:mm:ss}\n" +
                        $"우선순위: {GetPriorityText(memo.Priority)}\n" +
                        $"즐겨찾기: {(memo.IsFavorite ? "예" : "아니오")}\n\n" +
                        $"내용:\n{memo.Content}";

        File.WriteAllText(filePath, content);
    }

    private string GetPriorityText(int priority)
    {
        return priority switch
        {
            1 => "낮음",
            2 => "보통",
            3 => "높음",
            _ => "일반"
        };
    }
}
