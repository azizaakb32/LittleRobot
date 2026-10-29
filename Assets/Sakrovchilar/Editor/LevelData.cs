using System;
using System.Collections.Generic;

/// <summary>
/// Daraja ma'lumotlari. Levels/level_NN.json fayllari shu tuzilmada.
/// JSON'ni qo'lda tahrirlab, "Sakrovchilar > 4. Darajalarni JSON'dan qayta yaratish" ni bosing.
/// </summary>
[Serializable]
public class LevelPoint
{
    public float x;
    public float y;
}

[Serializable]
public class LevelBlock
{
    public string n;
    public float x0, y0, x1, y1;
    /// <summary>"ground", "platform" yoki "wall"</summary>
    public string kind;
}

[Serializable]
public class LevelMover
{
    public float ax, ay, bx, by;
    public float w;
    public float speed;
}

[Serializable]
public class LevelGear
{
    public float x, y, scale, speed;
}

[Serializable]
public class LevelEnemy
{
    public float x, y, minX, maxX, speed;
}

[Serializable]
public class LevelShooter
{
    public float x, y, interval, speed;
}

[Serializable]
public class LevelData
{
    public int number;
    public string world;
    public int worldFirst;
    public float par;
    public string bg, body, top, gear, moverBody, moverTop;
    public LevelPoint spawn;
    public LevelPoint boundsMin;
    public LevelPoint boundsMax;
    public LevelPoint key;
    public LevelPoint door;
    public List<LevelBlock> blocks;
    public List<LevelPoint> spikes;
    public List<LevelPoint> coins;
    public List<LevelMover> movers;
    public List<LevelGear> gears;
    public List<LevelEnemy> enemies;
    public List<LevelShooter> shooters;
}
