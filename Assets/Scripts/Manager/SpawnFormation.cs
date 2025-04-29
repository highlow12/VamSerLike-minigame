using UnityEngine;


// 몬스터 스폰 패턴의 종류를 정의하는 열거형
public enum SpawnPatternType
{
    Circle,      // 몬스터를 원형으로 배치
    Line,        // 몬스터를 직선으로 배치
    Triangle,    // 몬스터를 삼각형 꼭지점에 배치
    Square,      // 몬스터를 정사각형 꼭지점에 배치
    Random       // 몬스터를 랜덤한 위치에 배치
}

// 몬스터 스폰 패턴의 기본 추상 클래스
// 모든 구체적인 스폰 패턴은 이 클래스를 상속받아 구현
public abstract class SpawnFormation
{
    protected SpawnPatternData patternData;    // 패턴 생성에 필요한 기본 데이터
    protected float wiggle = 0;                // 몬스터 위치의 무작위성을 결정하는 변수

    public SpawnFormation(SpawnPatternData data, float wiggle = 0)
    {
        patternData = data;
        this.wiggle = wiggle;
    }

    public abstract Vector2[] GetSpawnPositions(Vector2 centerPosition);

    // Factory 메서드 - 패턴 타입에 따라 적절한 Formation 객체를 생성
    public static SpawnFormation CreateFormation(SpawnPatternData pattern, float wiggle = 0)
    {
        return pattern.patternType switch
        {
            SpawnPatternType.Circle => new CircleFormation(pattern, wiggle),
            SpawnPatternType.Square => new SquareFormation(pattern, wiggle),
            SpawnPatternType.Line => new LineFormation(pattern, wiggle),
            SpawnPatternType.Triangle => new TriangleFormation(pattern, wiggle),
            SpawnPatternType.Random => new RandomFormation(pattern, wiggle),
            _ => throw new System.NotImplementedException($"Formation type {pattern.patternType} not implemented")
        };
    }

    // 여러 Formation 클래스에서 공통으로 사용되는 유틸리티 메서드
    protected Vector2 ApplyWiggle(Vector2 position, Vector2 direction)
    {
        if (wiggle <= 0) return position;
        float randomOffset = Random.Range(-wiggle, wiggle);
        return position + direction * randomOffset;
    }

    // 변 위의 몬스터 위치를 계산하는 공통 메서드
    protected void PlaceMonstersOnSide(Vector2[] positions, int startIndex, int count,
                                       Vector2 startPoint, Vector2 endPoint, Vector2 centerPosition)
    {
        Vector2 sideDirection = (endPoint - startPoint).normalized;
        Vector2 perpendicular = Vector2.Perpendicular(sideDirection);

        for (int i = 0; i < count; i++)
        {
            int index = startIndex + i;
            if (index >= positions.Length) break;

            float t = (i + 1f) / (count + 1f);  // 보간 비율
            Vector2 basePosition = Vector2.Lerp(startPoint, endPoint, t);
            basePosition = ApplyWiggle(basePosition, perpendicular);

            positions[index] = centerPosition + basePosition;
        }
    }
}

// 몬스터를 랜덤하게 배치하는 Formation
public class RandomFormation : SpawnFormation
{
    public RandomFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        // 랜덤 스폰은 MonsterSpawner에서 처리하므로 null 대신 비어있는 배열 반환
        return new Vector2[0];
    }
}

// 몬스터를 원형으로 배치하는 Formation
public class CircleFormation : SpawnFormation
{
    public CircleFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[patternData.monsterCount];
        float angleStep = 360f / patternData.monsterCount;          // 몬스터 간의 각도 간격
        Quaternion baseRotation = Quaternion.Euler(0, 0, patternData.angle);    // 전체 패턴의 회전값

        for (int i = 0; i < patternData.monsterCount; i++)
        {
            // 각 몬스터의 회전각 계산 및 적용
            Quaternion rotation = Quaternion.Euler(0, 0, angleStep * i);
            // 기본 반지름에 랜덤 편차(wiggle) 적용
            float randomRadius = patternData.radius;
            Vector2 basePosition = rotation * new Vector2(0, randomRadius);
            basePosition = ApplyWiggle(basePosition, basePosition.normalized);
            // 최종 위치 계산 (기본 회전 + 전체 패턴 회전)
            positions[i] = centerPosition + (Vector2)(baseRotation * basePosition);
        }

        return positions;
    }
}

// 몬스터를 직선으로 배치하는 Formation
public class LineFormation : SpawnFormation
{
    public LineFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[patternData.monsterCount];
        float spacing = patternData.radius * 2 / Mathf.Max(1, patternData.monsterCount - 1);    // 몬스터 간의 간격
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);            // 전체 라인의 회전값

        for (int i = 0; i < patternData.monsterCount; i++)
        {
            float x = (-patternData.radius) + (spacing * i);    // x축 방향 위치
            Vector2 basePosition = new Vector2(x, 0);
            basePosition = ApplyWiggle(basePosition, new Vector2(0, 1)); // y축 방향으로 wiggle 적용
            positions[i] = centerPosition + (Vector2)(rotation * basePosition);
        }

        return positions;
    }
}

// 몬스터를 삼각형 꼭지점에 배치하는 Formation
public class TriangleFormation : SpawnFormation
{
    public TriangleFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[patternData.monsterCount];
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);    // 전체 삼각형의 회전값

        // 각 변당 몬스터 수 계산 (전체 몬스터 수를 3으로 나눔)
        int monstersPerSide = Mathf.Max(1, patternData.monsterCount / 3);
        Vector2[] corners = new Vector2[3];

        // 삼각형의 세 꼭지점 계산
        for (int i = 0; i < 3; i++)
        {
            corners[i] = (Quaternion.Euler(0, 0, i * 120f) * new Vector2(0, patternData.radius));
        }

        // 회전 적용
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = rotation * corners[i];
        }

        // 각 변에 몬스터 배치
        for (int side = 0; side < 3; side++)
        {
            int startIndex = side * monstersPerSide;
            PlaceMonstersOnSide(positions, startIndex, monstersPerSide,
                corners[side], corners[(side + 1) % 3], centerPosition);
        }

        return positions;
    }
}

// 몬스터를 정사각형 꼭지점에 배치하는 Formation
public class SquareFormation : SpawnFormation
{
    public SquareFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[patternData.monsterCount];
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);    // 전체 사각형의 회전값

        // 각 변당 몬스터 수 계산 (전체 몬스터 수를 4로 나눔)
        int monstersPerSide = Mathf.Max(1, patternData.monsterCount / 4);

        // 사각형의 네 꼭지점 정의
        Vector2[] corners = new Vector2[4] {
            new Vector2(-patternData.radius, patternData.radius),   // 좌상단
            new Vector2(patternData.radius, patternData.radius),    // 우상단
            new Vector2(patternData.radius, -patternData.radius),   // 우하단
            new Vector2(-patternData.radius, -patternData.radius)   // 좌하단
        };

        // 회전 적용
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = rotation * corners[i];
        }

        // 각 변에 몬스터 배치
        for (int side = 0; side < 4; side++)
        {
            int startIndex = side * monstersPerSide;
            PlaceMonstersOnSide(positions, startIndex, monstersPerSide,
                corners[side], corners[(side + 1) % 4], centerPosition);
        }

        return positions;
    }
}
