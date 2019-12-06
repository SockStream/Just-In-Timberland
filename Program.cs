using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Deliver more wood to truck (middle left side of the map) than your opponent. Use coffee and axe sharpener to outrun your opponent
 **/

enum Mode
{
    MODE_OPPORTUNISTE, //on se promène en tagant les arbres non taggés, si on trouve un arbre à 1hp, on le coupe, si on voit un arbre en cours de coupe, on passe en mode attente_vol
    MODE_ATTENTE_VOL_BOIS, //on wait jusqu'à ce que l'arbre soit coupé, une fois que l'arbre est coupé, si on l'a on passe en MODE_RETOUR_CAMION
    MODE_EXPLORATION, //on se promène en cherchant à explorer toute la carte, si on voit des champis on se pose dessus
    MODE_RETOUR_CAMION, //on pathfind jusqu'au camion
    MODE_DECOUPE, //on coupe l'arbre et si l'autre bucheron n'est pas loin on l'appelle pour sécuriser 
    MODE_COOPERATIF, //quand il n'y a plus d'arbres de niveau 1, on cherche les arbres de niveau supérieur en cherchant la rentabilité : A FAIRE
    MODE_CAISSE_CHAMPIGNON
}

enum Volonte
{
    TAGGER,
    VOLER,
    AUCUNE,
    COUPER
}

abstract class Cellule
{
    public bool Franchissable { get; set; }
    public bool SeFaitDecouper { get; internal set; }
}

abstract class CellulleFranchissable : Cellule
{
    public CellulleFranchissable()
    {
        Franchissable = true;
    }
}

class CelluleInconnue : CellulleFranchissable
{
    public CelluleInconnue()
    {

    }
}

class BonusEnergie : CellulleFranchissable
{
    public int Energie { get; internal set; }
}

class BonusCoupe : CellulleFranchissable
{
    public int NbTours { get; internal set; }
}

class CaisseChampignons : CellulleFranchissable
{

}
abstract class CellulleBloquante : Cellule
{
    public CellulleBloquante()
    {
        Franchissable = false;
    }

}

class Arbre : CellulleBloquante
{
    public int Energie
    {
        get;
        set;
    }
    public bool Tag
    {
        get;
        set;
    }

    public Arbre()
    {

    }

    public bool estCoupe()
    {
        return Energie == 0;
    }
}

class Camion : CellulleFranchissable
{
}

class Obstacle : CellulleBloquante
{

}
class Carte
{
    public Cellule[,] tableau = null;
    public int MyScore { get; set; }
    public int EnnemyScore { get; set; }
    public int TagLeft { get; set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int NbTags { get; internal set; }

    public AbstractBucheron _Pathfinder = null;
    public AbstractBucheron _Opportunity = null;
    public AbstractBucheron Ennemi1 = null;
    public AbstractBucheron Ennemi2 = null;
    private static Carte INSTANCE = null;

    public static Carte GetInstance()
    {
        if (INSTANCE == null)
        {
            INSTANCE = new Carte();
        }
        return INSTANCE;
    }

    public void MettreAJourBucheron(int id, int playerId, int x, int y, int energy, int axeBonus, int inventory)
    {
        Console.Error.WriteLine("mise a jour Bucheron : " + id + "," + playerId);
        //ce sont les miens
        AbstractBucheron bucheron;
        if (playerId == 0)
        {
            //Initialisation de mes bucherons
            if (_Pathfinder == null)
            {
                _Pathfinder = new PathFinder();

                _Pathfinder.Id = id;
            }
            else if (_Opportunity == null)
            {
                _Opportunity = new Opportunity();
                _Opportunity.Id = id;
            }

            if (_Pathfinder.Id == id)
            {
                bucheron = _Pathfinder;
            }
            else if (_Opportunity.Id == id)
            {
                bucheron = _Opportunity;
            }
            else
            {
                throw new Exception("J'ai un bucheron non r�pertori� !!!");
            }
        }
        else //mort aux ennemis !!!
        {
            //Initialisation des bucherons ennemis
            if (Ennemi1 == null)
            {
                Ennemi1 = new Bucheron();
                Ennemi1.Id = id;
            }
            else if (Ennemi2 == null)
            {
                Ennemi2 = new Bucheron();
                Ennemi2.Id = id;
            }

            if (Ennemi1.Id == id)
            {
                bucheron = Ennemi1;
            }
            else if (Ennemi2.Id == id)
            {
                bucheron = Ennemi2;
            }
            else
            {
                throw new Exception("J'ai un bucheron ennemi non r�pertori� !!!");
            }
        }

        //mise a jour effective
        if (bucheron != null)
        {
            bucheron._X = x;
            bucheron._Y = y;
            bucheron.Energy = energy;
            bucheron.AxeBonus = axeBonus;
            bucheron.inventory = inventory;
        }
    }

    internal void MettreAJourTableau(int entityType, int x, int y, int amount)
    {
        Cellule cell = tableau[x, y];
        switch (entityType)
        {
            case 2: //arbre
                if (cell == null)
                {
                    Console.Error.WriteLine("Arbre a  x:" + x + " y:" + y);
                    cell = new Arbre();
                }
                if (((Arbre)cell).Energie > amount)
                {
                    Console.Error.WriteLine("un arbre se fait couper en x:" + x + " y:" + y);
                    cell.SeFaitDecouper = true;
                }
                    ((Arbre)cell).Energie = amount;
                break;
            case 3: //souche
                cell = new Obstacle();
                break;
            case 4: //obstacle
                if (cell == null)
                {
                    cell = new Obstacle();
                }
                break;
            case 5: //caisse de champignons
                break;
            case 7: //Bonus Energie
                if (cell == null)
                {
                    cell = new BonusEnergie();
                    ((BonusEnergie)cell).Energie = amount;
                }
                break;
            case 8: //Bonus Coupe
                if (cell == null)
                {
                    cell = new BonusCoupe();
                    ((BonusCoupe)cell).NbTours = amount;
                }
                break;
            default:
                throw new Exception("EntityType inconnu : " + entityType + " x:" + x + " y:" + y );
        }
        tableau[x, y] = cell;
        if (tableau[x, y] == null)
        {
            throw new Exception("le tableau ne s'est pas mis à jour !!!");
        }
    }

    internal void InitialiserTableau(int width, int height)
    {
        Width = width;
        Height = height;
        tableau = new Cellule[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tableau[i, j] = null;
            }
        }
        //bof bof utile ?
        //camion
        tableau[0, 7] = new Camion();
        //caisse
        tableau[29, 7] = new CaisseChampignons();
    }
}

abstract class AbstractBucheron
{
    public int Energy { get; set; }
    public int Id { get; set; }
    public int _X { get; set; }
    public int _Y { get; set; }
    public int AxeBonus { get; set; }
    public int inventory { get; set; }

    protected Mode _mode;
    protected Mode _modeDefaut;

    protected int[] cible = null;

    protected String nom = String.Empty;
    private Volonte _Volonte;

    public string DefinirAction()
    {
        String action = null;

        if (_X == 0 && _Y == 7 && _mode == Mode.MODE_RETOUR_CAMION)
        {
            _mode = _modeDefaut;
        }
        switch (_mode)
        {
            case Mode.MODE_ATTENTE_VOL_BOIS:
                break;
            case Mode.MODE_COOPERATIF:
                break;
            case Mode.MODE_DECOUPE:
                break;
            case Mode.MODE_EXPLORATION:
                action = PathFinding(29, 7);
                break;
            case Mode.MODE_OPPORTUNISTE:
                action = PathFinding(29, 7);
                if (inventory > 0)//si on a qqch dans l'inventaire
                {
                    action = PathFinding_Camion();//path_finding_camion
                    _mode = Mode.MODE_RETOUR_CAMION; //Mode = retour_camion;
                    _Volonte = Volonte.AUCUNE;
                }
                if (cible != null)
                {
                    Cellule cellule = Carte.GetInstance().tableau[cible[0], cible[1]];
                    if (cible != null && cellule is Arbre && !((Arbre)cellule).Tag)//si la cible est un arbre non taggé
                    {
                        if (_Volonte == Volonte.TAGGER) //si on veut le tagger
                        {
                            if (Carte.GetInstance().NbTags <= 0)
                            {
                                cible = null;
                                _Volonte = Volonte.AUCUNE;
                            }
                            if (((Arbre)cellule).SeFaitDecouper)//si l'arbre est en cours de découpe => on ne peut plus le tagger
                            {
                                _Volonte = Volonte.VOLER; //on va essayer de voler l'arbre
                            }
                            if (CibleEstACote()) //si je suis à côté de mon arbre
                            {
                                ((Arbre)cellule).Tag = true;
                                action = "TAG " + cible[0] + " " + cible[1]; //je le tagge
                                cible = null;
                                return action;
                            }
                            else
                            {
                                action = PathFinding(cible[0], cible[1]);
                                return action;
                            }
                        }
                        if (_Volonte == Volonte.VOLER)//si on veut le voler
                        {
                            if (CibleEstACote())//si on est à côté de l'arbre //TODO vérifier qu'il n'y a que des coupeurs à côté ?
                            {
                                action = "WAIT";
                                return action;
                            }
                            else
                            {
                                action = PathFinding(cible[0], cible[1]);
                                _Volonte = Volonte.VOLER;
                                return action;
                            }
                        }
                        if (_Volonte == Volonte.COUPER)
                        {
                            if (CibleEstACote())//si on est à côté de l'arbre
                            {
                                action = "CUT " + cible[0] + " " + cible[1];
                                return action;
                            }
                            else
                            {
                                action = PathFinding(cible[0], cible[1]);
                                return action;
                            }
                        }
                    }
                    else if (cible != null && cellule is Obstacle) //sinon si la cible est un obstacle
                    {
                        cible = null; //on a plus de cible
                        _Volonte = Volonte.AUCUNE; //on ne cherche plus à voler ou à tagger
                    }
                }

                if (JePeuxVolerArbre())
                {
                    if (CibleEstACote())//si on est à côté de l'arbre //TODO vérifier qu'il n'y a que des coupeurs à côté ?
                    {
                        action = "WAIT";
                        return action;
                    }
                    else
                    {
                        action = PathFinding(cible[0], cible[1]);
                        _Volonte = Volonte.VOLER;
                        return action;
                    }
                }
                if (IlExisteUnArbreTaggableACote())
                {
                    if (CibleEstACote())
                    {
                        Cellule cellule = Carte.GetInstance().tableau[cible[0], cible[1]];
                        ((Arbre)cellule).Tag = true;
                        action = "TAG " + cible[0] + " " + cible[1]; //je le tagge
                        cible = null;
                        return action;
                    }
                    else
                    {
                        action = PathFinding(cible[0], cible[1]);
                        return action;
                    }
                }
                if (IlExisteUnArbreCoupableACote())
                {
                    if (CibleEstACote())
                    {
                        action = "CUT " + cible[0] + " " + cible[1]; //je le coupe
                    }
                    else
                    {
                        action = PathFinding(cible[0], cible[1]);
                        _Volonte = Volonte.COUPER;
                    }
                }
                break;
            case Mode.MODE_RETOUR_CAMION :
                action = PathFinding_Camion();
                break;
            case Mode.MODE_CAISSE_CHAMPIGNON:
                action = PathFinding_CaisseChampignon();
                break;
            default:
                throw new Exception(nom + " mode non géré : " + _mode);
        }
        if (action == null)
        {
            throw new Exception(nom + " : n'a pas trouvé quoi faire");
        }
        return action;
    }

    private bool IlExisteUnArbreCoupableACote()
    {
        bool JePeuxCouper = false;
        for (int i = _X - 5; i < _X + 5; i++)
        {
            for (int j = _Y - 5; j < _Y + 5; j++)
            {
                if (i > 0 && i < Carte.GetInstance().Width && j > 0 && j < Carte.GetInstance().Height)
                {
                    Cellule cellule = Carte.GetInstance().tableau[i, j];
                    if (cellule is Arbre && ((Arbre)cellule).Energie <= 2 && !((Arbre)cellule).SeFaitDecouper)
                    {
                        cible = new int[] { 0, 0 };
                        cible[0] = i;
                        cible[1] = j;
                        return true;
                    }
                }
            }
        }
        return JePeuxCouper;
    }

    private bool IlExisteUnArbreTaggableACote()
    {
        bool JePeuxTagger = false;
        for (int i = _X - 5; i < _X + 5; i++)
        {
            for (int j = _Y - 5; j < _Y + 5; j++)
            {
                if (i > 0 && i < Carte.GetInstance().Width && j > 0 && j < Carte.GetInstance().Height)
                {
                    Cellule cellule = Carte.GetInstance().tableau[i, j];
                    if (cellule is Arbre && !((Arbre)cellule).Tag && !((Arbre)cellule).SeFaitDecouper)
                    {
                        cible = new int[] { 0, 0 };
                        cible[0] = i;
                        cible[1] = j;
                        return true;
                    }
                }
            }
        }
        return JePeuxTagger;
    }

    ///<summary>
    ///Renvoie un booleen pour indiquer si on peut voler un arbre, et change la cible du bucheron si c'est le cas
    ///</summary>
    private bool JePeuxVolerArbre()
    {
        bool JePeuxVoler = false;
        for (int i = _X-5; i<_X+5; i++)
        {
            for (int j = _Y-5; j<_Y+5; j++)
            {
                if (i > 0 && i < Carte.GetInstance().Width && j > 0 && j < Carte.GetInstance().Height)
                {
                    Cellule cellule = Carte.GetInstance().tableau[i, j];
                    if (cellule is Arbre && ( Carte.GetInstance().Ennemi1.EstACote(i,j) || Carte.GetInstance().Ennemi2.EstACote(i,j)) && ((Arbre)cellule).Energie <=4 && !((Arbre)cellule).SeFaitDecouper)
                    {
                        cible = new int[] { 0, 0 };
                        cible[0] = i;
                        cible[1] = j;
                        return true;
                    }
                }
            }
        }
        return JePeuxVoler;
    }

    private bool CibleEstACote()
    {
        if (cible == null)
        {
            throw new Exception("CibleEstACote sans Cible");
        }
        return EstACote(cible[0], cible[1]);
    }

    private bool EstACote(int x, int y)
    {
        bool ACote = false;

        if (_X - 1 > 0 && x == (_X - 1) && y == _Y)
        {
            ACote = true;
        }
        if (_X + 1 < Carte.GetInstance().Width && x == (_X + 1) && y == _Y)
        {
            ACote = true;
        }
        if (_Y - 1 > 0 && x == _X && y == (_Y - 1))
        {
            ACote = true;
        }
        if (_Y + 1 < Carte.GetInstance().Height && x == _X && y == (_Y + 1))
        {
            ACote = true;
        }


        return ACote;
    }


    protected String PathFinding_Camion()
    {
        return PathFinding(0, 7);
    }

    protected String PathFinding_CaisseChampignon()
    {
        return PathFinding(29, 7);
    }

    protected String PathFinding(int x, int y)
    {
        return "MOVE " + x + " " + y;
    }
}

class Bucheron : AbstractBucheron
{
    public Bucheron()
    {
        Console.Error.WriteLine("Creation d'un Bucheron ennemi");
    }
}

class PathFinder : AbstractBucheron
{
    public PathFinder()
    {
        Console.Error.WriteLine("Hello PathFinder");
        _mode = Mode.MODE_OPPORTUNISTE;
        _modeDefaut = Mode.MODE_OPPORTUNISTE;
        nom = "PathFinder";
    }
}

class Opportunity : AbstractBucheron
{

    public Opportunity()
    {
        Console.Error.WriteLine("Hello Opportunity");
        _mode = Mode.MODE_OPPORTUNISTE;
        _modeDefaut = Mode.MODE_OPPORTUNISTE;
        nom = "Opportunity";
    }
}

class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        Console.Error.WriteLine("width : " + width);
        Console.Error.WriteLine("height : " + height);
        int N = int.Parse(Console.ReadLine());
        Console.Error.WriteLine("N : " + N);
        for (int i = 0; i < N; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int cellType = int.Parse(inputs[0]); // 0 for truck, 1 for energy cells
            int x = int.Parse(inputs[1]); // position of the entity
            int y = int.Parse(inputs[2]); // position of the entity
            Console.Error.WriteLine("cellType : " + cellType);
            Console.Error.WriteLine("x : " + x);
            Console.Error.WriteLine("y : " + y);
        }

        //
        Carte carte = Carte.GetInstance();
        carte.InitialiserTableau(width, height);
        carte.NbTags = N;

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]); // Amount of trees delivered
            int opponentScore = int.Parse(inputs[1]);
            int tagsLeft = int.Parse(inputs[2]);
            Console.Error.WriteLine("myScore : " + myScore);
            Console.Error.WriteLine("opponentScore : " + opponentScore);
            Console.Error.WriteLine("tagsLeft : " + tagsLeft);
            carte.MyScore = myScore;
            carte.TagLeft = tagsLeft;

            for (int i = 0; i < 4; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // unique id of the entity
                int playerId = int.Parse(inputs[1]); // 0 for your lumberjack, 1 for other lumberjack
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int energy = int.Parse(inputs[4]); // energy meter of the lumberjack
                int axeBonus = int.Parse(inputs[5]); // while > 0, each cut count twice
                int inventory = int.Parse(inputs[6]); // 0 if empty, else lumberjack carry a tree
                carte.MettreAJourBucheron(id, playerId, x, y, energy, axeBonus, inventory);
                Console.Error.WriteLine("id : " + id);
                Console.Error.WriteLine("playerId : " + playerId);
                Console.Error.WriteLine("x : " + x);
                Console.Error.WriteLine("y : " + y);
                Console.Error.WriteLine("energy : " + energy);
                Console.Error.WriteLine("axeBonus : " + axeBonus);
                Console.Error.WriteLine("inventory : " + inventory);
            }
            int entityCount = int.Parse(Console.ReadLine()); // number of entities visible to you
            Console.Error.WriteLine("entityCount : " + entityCount);
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]);
                int entityType = int.Parse(inputs[1]); // 2 for tree, 3 for stump, 4 for fence, 7 for energy bonus, 8 for axe bonus
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int amount = int.Parse(inputs[4]); // depends of type (see rules)
                carte.MettreAJourTableau(entityType, x, y, amount);
            }

            Console.WriteLine(carte._Pathfinder.DefinirAction());
            Console.WriteLine(carte._Opportunity.DefinirAction());
            //for (int i = 0; i < 2; i++)
            //{

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                //Console.WriteLine("WAIT"); // WAIT|MOVE x y|CUT x y|TAG x y

            //}
        }
    }
}