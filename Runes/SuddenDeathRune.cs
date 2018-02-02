public class SuddenDeathRune : AttackSingleTargetRune
{
    override protected int damage
    {
        get
        {
            return 30;
        }
    }

    override protected string element
    {
        get
        {
            return "death";
        }
    }

    override public void use(Player player)
    {
        base.use(player);
    }
}
