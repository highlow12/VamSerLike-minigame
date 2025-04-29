using System.Collections.Generic;

/// <summary>
/// 저장 및 로드를 위해 객체의 상태를 캡처하고 복원하는 메서드를 정의합니다.
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// 객체의 현재 상태를 직렬화 가능한 형식으로 캡처합니다.
    /// </summary>
    /// <returns>저장할 상태를 나타내는 객체. 직렬화 가능해야 합니다.</returns>
    object CaptureState();

    /// <summary>
    /// 제공된 상태 데이터로부터 객체의 상태를 복원합니다.
    /// </summary>
    /// <param name="state">이전에 CaptureState에서 반환된 상태 객체.</param>
    void RestoreState(object state);

    /// <summary>
    /// 이 저장 가능한 객체에 대한 고유 식별자를 가져옵니다.
    /// 이 ID는 게임 세션 간(예: 씬 로드)에 일관성이 있어야 합니다.
    /// </summary>
    /// <returns>고유한 문자열 식별자.</returns>
    string GetUniqueIdentifier();
}
