using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ProbabilityList : List<float>
{
    private bool m_normalized = false;
    public ProbabilityList(IEnumerable<float> collection) : base(collection)
    {
        onValueAccess += () =>
        {
            if (!m_normalized)
            {
                Normalize();
            }
        };
        onValueChanged += () =>
        {
            m_normalized = false;
        };
    }

    // 데이터 변경 이벤트
    public delegate void OnValueChanged();
    public event OnValueChanged onValueChanged;

    //데이터 접근 이벤트
    public delegate void OnValueAccess();
    public event OnValueAccess onValueAccess;

    // 인덱서 정의
    public new float this[int index]
    {
        get
        {
            onValueAccess?.Invoke();
            return base[index];
        }
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("확률은 0보다 커야합니다.");
            }
            base[index] = value;
            onValueChanged?.Invoke();
        }
    }

    private void Normalize()
    {
        // 인덱서에 의한 재귀 호출을 방지하기 위해, base에 직접 접근하여 값을 수정
        float sum = ToArray().Sum();
        for (int i = 0; i < Count; i++)
        {
            base[i] = base[i] / sum;
        }
        m_normalized = true;
    }

    public new void Add(float item)
    {
        base.Add(item);
        onValueChanged?.Invoke();
    }

    public new void Remove(float item)
    {
        base.Remove(item);
        onValueChanged?.Invoke();
    }
#if UNITY_EDITOR
    public string GetElements()
    {
        string elements = "";
        for (int i = 0; i < Count; i++)
        {
            elements += this[i] + " ";
        }
        return elements;
    }

    // 문자열을 ProbabilityList로 변환하는 정적 메서드
    public static ProbabilityList CustomParse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("입력 문자열이 비어 있습니다.");

        // 쉼표로 구분된 문자열을 분리하여 float로 파싱
        string[] parts = s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        List<float> values = new();

        foreach (string part in parts)
        {
            if (float.TryParse(part.Trim(), out float result))
            {
                if (result <= 0)
                    throw new ArgumentException("모든 확률 값은 0보다 커야합니다.");
                values.Add(result);
            }
            else
            {
                throw new FormatException($"'{part}' 값은 부동소수점 형식으로 파싱할 수 없습니다.");
            }
        }

        return new ProbabilityList(values);
    }
#endif
}
