using System.Collections;

public class PropellerFly : PrimaryMovement
{
    private void Awake()
    {
        PreferredCursor = CursorType.AIR;
        Style = MoveStyle.FLY;
    }

    public override IEnumerator NeutralStance()
    {
        yield return null;
    }
}
