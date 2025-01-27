using UnityEngine;

/// <summary>
/// 몬스터 스폰 패턴의 종류를 정의하는 열거형
/// </summary>
public enum SpawnPatternType
{
    Circle,      // 몬스터를 원형으로 배치
    Line,        // 몬스터를 직선으로 배치
    Triangle,    // 몬스터를 삼각형 꼭지점에 배치
    Square       // 몬스터를 정사각형 꼭지점에 배치
}

/// <summary>
/// 몬스터 스폰 패턴의 기본 추상 클래스
/// 모든 구체적인 스폰 패턴은 이 클래스를 상속받아 구현
/// </summary>
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
}

/// <summary>
/// 몬스터를 원형으로 배치하는 Formation
/// 중심점을 기준으로 일정한 각도 간격으로 몬스터를 배치
/// wiggle: 각 몬스터의 반지름 거리에 적용되는 랜덤 범위
/// </summary>
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
            float randomRadius = patternData.radius + Random.Range(-wiggle, wiggle);
            Vector2 basePosition = rotation * new Vector2(0, randomRadius);
            // 최종 위치 계산 (기본 회전 + 전체 패턴 회전)
            positions[i] = centerPosition + (Vector2)(baseRotation * basePosition);
        }
        
        return positions;
    }
}

/// <summary>
/// 몬스터를 직선으로 배치하는 Formation
/// 중심점을 기준으로 좌우로 일정 간격으로 몬스터를 배치
/// wiggle: 기준선으로부터 수직 방향으로의 랜덤 범위
/// </summary>
public class LineFormation : SpawnFormation
{
    public LineFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[patternData.monsterCount];
        float spacing = patternData.radius * 2 / (patternData.monsterCount - 1);    // 몬스터 간의 간격
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);            // 전체 라인의 회전값
        
        for (int i = 0; i < patternData.monsterCount; i++)
        {
            float x = (-patternData.radius) + (spacing * i);    // x축 방향 위치
            float randomY = Random.Range(-wiggle, wiggle);      // y축 방향 랜덤 편차
            Vector2 basePosition = new Vector2(x, randomY);
            positions[i] = centerPosition + (Vector2)(rotation * basePosition);
        }
        
        return positions;
    }
}

/// <summary>
/// 몬스터를 삼각형 꼭지점에 배치하는 Formation
/// 중심점을 기준으로 120도 간격으로 몬스터를 배치
/// wiggle: 각 꼭지점의 중심으로부터의 거리에 적용되는 랜덤 범위
/// </summary>
public class TriangleFormation : SpawnFormation
{
    public TriangleFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[3];
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);    // 전체 삼각형의 회전값
        
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f;    // 120도 간격으로 꼭지점 배치
            float randomRadius = patternData.radius + Random.Range(-wiggle, wiggle);    // 랜덤 반지름
            Vector2 basePosition = Quaternion.Euler(0, 0, angle) * new Vector2(0, randomRadius);
            positions[i] = centerPosition + (Vector2)(rotation * basePosition);
        }
        
        return positions;
    }
}

/// <summary>
/// 몬스터를 정사각형 꼭지점에 배치하는 Formation
/// 중심점을 기준으로 네 모서리에 몬스터를 배치
/// wiggle: 각 꼭지점의 중심으로부터의 거리에 적용되는 랜덤 범위
/// </summary>
public class SquareFormation : SpawnFormation
{
    public SquareFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[4];
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);    // 전체 사각형의 회전값
        
        for (int i = 0; i < 4; i++)
        {
            float randomOffset = Random.Range(-wiggle, wiggle);     // 랜덤 거리 편차
            float halfRadius = (patternData.radius / 2f) + randomOffset;
            
            // 각 꼭지점의 기본 위치 설정
            Vector2 basePosition = i switch
            {
                0 => new Vector2(-halfRadius, halfRadius),    // 좌상단
                1 => new Vector2(halfRadius, halfRadius),     // 우상단
                2 => new Vector2(halfRadius, -halfRadius),    // 우하단
                _ => new Vector2(-halfRadius, -halfRadius)    // 좌하단
            };
            
            positions[i] = centerPosition + (Vector2)(rotation * basePosition);
        }
        
        return positions;
    }
}
