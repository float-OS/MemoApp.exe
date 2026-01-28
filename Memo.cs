using System;

namespace MemoApp;

/// <summary>
/// 메모 데이터 모델
/// </summary>
public class Memo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public string Category { get; set; } = "기본";
    public bool IsFavorite { get; set; } = false;
    public string Tags { get; set; } = string.Empty;
    public int Priority { get; set; } = 0; // 0: 일반, 1: 낮음, 2: 보통, 3: 높음
}
