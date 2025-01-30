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
}


// 몬스터를 랜덤하게 배치하는 Formation
// MonsterSpawner의 랜덤 스폰 기능을 사용하여 몬스터를 무작위로 배치
// wiggle: 사용되지 않음 (MonsterSpawner의 내부 설정 사용)

public class RandomFormation : SpawnFormation
{
    public RandomFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) 
    {
        MonsterSpawner.Instance.StartRandomSpawning(data.monsterName);
    }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        return null;
    }
}


// 몬스터를 원형으로 배치하는 Formation
// 중심점을 기준으로 일정한 각도 간격으로 몬스터를 배치
// wiggle: 각 몬스터의 반지름 거리에 적용되는 랜덤 범위

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


// 몬스터를 직선으로 배치하는 Formation
// 중심점을 기준으로 좌우로 일정 간격으로 몬스터를 배치
// wiggle: 기준선으로부터 수직 방향으로의 랜덤 범위

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


// 몬스터를 삼각형 꼭지점에 배치하는 Formation
// 중심점을 기준으로 120도 간격으로 몬스터를 배치
// wiggle: 각 꼭지점의 중심으로부터의 거리에 적용되는 랜덤 범위

public class TriangleFormation : SpawnFormation
{
    public TriangleFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[patternData.monsterCount];
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);    // 전체 삼각형의 회전값
        
        // 각 변당 몬스터 수 계산 (전체 몬스터 수를 3으로 나눔)
        int monstersPerSide = patternData.monsterCount / 3;
        float sideLength = patternData.radius * 2f;  // 삼각형 한 변의 길이
        float spacing = sideLength / (monstersPerSide + 1);  // 몬스터 간 간격
        
        for (int side = 0; side < 3; side++)
        {
            Vector2 startPoint = Quaternion.Euler(0, 0, side * 120f) * new Vector2(0, patternData.radius);
            Vector2 endPoint = Quaternion.Euler(0, 0, (side + 1) * 120f) * new Vector2(0, patternData.radius);
            
            for (int i = 0; i < monstersPerSide; i++)
            {
                int index = side * monstersPerSide + i;
                float t = (i + 1f) / (monstersPerSide + 1f);  // 보간 비율
                Vector2 basePosition = Vector2.Lerp(startPoint, endPoint, t);
                
                // wiggle 적용 (변에 수직인 방향으로)
                Vector2 perpendicular = Vector2.Perpendicular((endPoint - startPoint).normalized);
                float randomOffset = Random.Range(-wiggle, wiggle);
                basePosition += perpendicular * randomOffset;
                
                positions[index] = centerPosition + (Vector2)(rotation * basePosition);
            }
        }
        
        return positions;
    }
}


// 몬스터를 정사각형 꼭지점에 배치하는 Formation
// 중심점을 기준으로 네 모서리에 몬스터를 배치
// wiggle: 각 꼭지점의 중심으로부터의 거리에 적용되는 랜덤 범위

public class SquareFormation : SpawnFormation
{
    public SquareFormation(SpawnPatternData data, float wiggle = 0) : base(data, wiggle) { }

    public override Vector2[] GetSpawnPositions(Vector2 centerPosition)
    {
        Vector2[] positions = new Vector2[patternData.monsterCount];
        Quaternion rotation = Quaternion.Euler(0, 0, patternData.angle);    // 전체 사각형의 회전값
        
        // 각 변당 몬스터 수 계산 (전체 몬스터 수를 4로 나눔)
        int monstersPerSide = patternData.monsterCount / 4;
        float sideLength = patternData.radius * 2f;  // 사각형 한 변의 길이
        float spacing = sideLength / (monstersPerSide + 1);  // 몬스터 간 간격
        
        // 사각형의 네 꼭지점 정의
        Vector2[] corners = new Vector2[4] {
            new Vector2(-patternData.radius, patternData.radius),   // 좌상단
            new Vector2(patternData.radius, patternData.radius),    // 우상단
            new Vector2(patternData.radius, -patternData.radius),   // 우하단
            new Vector2(-patternData.radius, -patternData.radius)   // 좌하단
        };
        
        for (int side = 0; side < 4; side++)
        {
            Vector2 startPoint = corners[side];
            Vector2 endPoint = corners[(side + 1) % 4];
            
            for (int i = 0; i < monstersPerSide; i++)
            {
                int index = side * monstersPerSide + i;
                float t = (i + 1f) / (monstersPerSide + 1f);  // 보간 비율
                Vector2 basePosition = Vector2.Lerp(startPoint, endPoint, t);
                
                // wiggle 적용 (변에 수직인 방향으로)
                Vector2 perpendicular = Vector2.Perpendicular((endPoint - startPoint).normalized);
                float randomOffset = Random.Range(-wiggle, wiggle);
                basePosition += perpendicular * randomOffset;
                
                positions[index] = centerPosition + (Vector2)(rotation * basePosition);
            }
        }
        
        return positions;
    }
}
