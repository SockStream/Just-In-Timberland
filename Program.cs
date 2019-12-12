using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Deliver more wood to truck (middle left side of the map) than your opponent. Use coffee and axe sharpener to outrun your opponent
 **/
namespace JustInTimberland
{

    public class Noeud
    {
        public int cout { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public List<Noeud> fils { get; set; }

        public Noeud parent { get; set; }
        public Noeud()
        {
            fils = new List<Noeud>();
        }
    }

    public class PathFinder
    {
        public static List<Noeud> Solve(int XOrigine, int YOrigine, int XDestination, int YDestination, bool Franchissable)
        {
            Console.Error.WriteLine("PathFinding de " + XOrigine + "," + YOrigine + " vers " + XDestination + "," + YDestination);
            bool found = false;
            List<Noeud> NoeudsParcourus = new List<Noeud>();
            List<Noeud> chemin = new List<Noeud>();
            List<Noeud> AExplorer = new List<Noeud>();
            Noeud racine = new Noeud();
            racine.x = XOrigine;
            racine.y = YOrigine;
            racine.cout = 0;
            racine.parent = null;
            Noeud destination = new Noeud();
            destination.x = XDestination;
            destination.y = YDestination;

            AExplorer.Add(racine);
            while (!found)
            {
                if (AExplorer.Count() == 0) //on est dans le cas d'un objet vraiment inateignable
                {
                    return null;
                }
                //on récupère en dépilant
                Noeud _noeud = AExplorer[0];
                AExplorer.RemoveAt(0);
                if (NoeudsParcourus.Where(n => n.x == _noeud.x && n.y == _noeud.y && n.cout <= _noeud.cout).Count() > 0)
                {
                    //Console.Error.WriteLine("ne rien faire");
                }
                else
                {
                    NoeudsParcourus.Add(_noeud);
                    if ((_noeud.x == XDestination && _noeud.y == YDestination) &&Franchissable)
                    {
                        destination = _noeud;
                        found = true;
                    }
                    else if (!Franchissable && Carte.GetInstance().EstACote(_noeud.x, _noeud.y, XDestination, YDestination))
                    {
                        destination = _noeud;
                        found = true;
                    }
                    else
                    { 
                        foreach (Noeud voisin in (ObtenirVoisins(_noeud)))
                        {
                            voisin.cout = _noeud.cout + 1;
                            voisin.parent = _noeud;
                            AExplorer.Add(voisin);
                        }
                    }
                }

            }


            Noeud noeud = destination;
            while(noeud != null)
            {
                chemin.Insert(0, noeud);
                noeud = noeud.parent;
            }
            if (chemin.Count() > 0)
            {
                chemin.RemoveAt(0);
            }
            return chemin;
        }

        private static IEnumerable<Noeud> ObtenirVoisins(Noeud noeud)
        {
            List<Noeud> voisins = new List<Noeud>();
            Cellule[,] tableau = Carte.GetInstance().tableau;

            if (noeud.x - 1 >= 0)
            {
                Cellule cellule = tableau[noeud.x - 1, noeud.y];
                if (cellule == null || (cellule != null && cellule.Franchissable))
                {
                    Noeud child = new Noeud();
                    child.x = noeud.x - 1;
                    child.y = noeud.y;
                    voisins.Add(child);
                }
            }

            if (noeud.x + 1 < Carte.GetInstance().Width)
            {
                Cellule cellule = tableau[noeud.x + 1, noeud.y];
                if (cellule == null || (cellule != null && cellule.Franchissable))
                {
                    Noeud child = new Noeud();
                    child.x = noeud.x + 1;
                    child.y = noeud.y;
                    voisins.Add(child);
                }
            }

            if (noeud.y - 1 >= 0)
            {
                Cellule cellule = tableau[noeud.x, noeud.y - 1];
                if (cellule == null || (cellule != null && cellule.Franchissable))
                {
                    Noeud child = new Noeud();
                    child.x = noeud.x;
                    child.y = noeud.y - 1;
                    voisins.Add(child);
                }
            }

            if (noeud.y + 1 < Carte.GetInstance().Height)
            {
                Cellule cellule = tableau[noeud.x, noeud.y + 1];
                if (cellule == null || (cellule != null && cellule.Franchissable))
                {
                    Noeud child = new Noeud();
                    child.x = noeud.x;
                    child.y = noeud.y + 1;
                    voisins.Add(child);
                }
            }
            return voisins;
        }
    }


    public enum Mode
    {
        MODE_OPPORTUNISTE, //on se promène en tagant les arbres non taggés, si on trouve un arbre à 1hp, on le coupe, si on voit un arbre en cours de coupe, on passe en mode attente_vol
        MODE_ATTENTE_VOL_BOIS, //on wait jusqu'à ce que l'arbre soit coupé, une fois que l'arbre est coupé, si on l'a on passe en MODE_RETOUR_CAMION
        MODE_EXPLORATION, //on se promène en cherchant à explorer toute la carte, si on voit des champis on se pose dessus
        MODE_RETOUR_CAMION, //on pathfind jusqu'au camion
        MODE_DECOUPE, //on coupe l'arbre et si l'autre bucheron n'est pas loin on l'appelle pour sécuriser 
        MODE_COOPERATIF, //quand il n'y a plus d'arbres de niveau 1, on cherche les arbres de niveau supérieur en cherchant la rentabilité : A FAIRE
        MODE_CAISSE_CHAMPIGNON
    }

    public enum Intension
    {
        TAGGER,
        VOLER,
        AUCUNE,
        COUPER
    }

    public abstract class Cellule
    {
        public int X, Y;
        public bool Franchissable { get; set; }
        public Cellule(int x, int y)
        {
            X = x;
            Y = y;
            Bloque = false;
        }
        public bool Securise
        {
            get
            {
                AbstractBucheron Curiosity = Carte.GetInstance().Curiosity;
                AbstractBucheron _Opportunity = Carte.GetInstance()._Opportunity;
                if (Carte.GetInstance().EstACote(Curiosity._X, Curiosity._Y, this.X, this.Y) && Carte.GetInstance().EstACote(Curiosity._X, Curiosity._Y, this.X, this.Y))
                {
                    return true;
                }
                return false;
            }
        }

        public bool Bloque { get; internal set; }
    }

    public abstract class CellulleFranchissable : Cellule
    {
        public CellulleFranchissable(int x, int y) : base(x,y)
        {
            Franchissable = true;
        }
    }

    public class CelluleInconnue : CellulleFranchissable
    {
        public CelluleInconnue(int x, int y) : base(x, y)
        {

        }
    }

    abstract public class Bonus : CellulleFranchissable
    {
        public bool Visible;

        protected Bonus(int x, int y) : base(x, y)
        {

        }
    }

    public class BonusEnergie : Bonus
    {
        public int Energie { get; internal set; }

        public BonusEnergie(int x, int y) : base(x, y)
        {

        }
    }

    public class BonusCoupe : Bonus
    {
        public int AxeBonus { get; internal set; }

        public BonusCoupe(int x, int y) : base(x, y)
        {

        }
    }

    public class CaisseChampignons : BonusEnergie
    {
        public CaisseChampignons(int x, int y) : base(x, y)
        {
            Energie = 100;
        }
    }
    public abstract class CellulleBloquante : Cellule
    {
        public CellulleBloquante(int x, int y) : base(x, y)
        {
            Franchissable = false;
        }

    }

    public class Arbre : CellulleBloquante
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
        public bool EstSquatte { get; internal set; }
        public bool CoupeParMoi { get; internal set; }
        public bool Entamme { get; internal set; }
        private bool _SeFaitDecouper;
        public int Risque = 0;

        public bool SeFaitDecouper {
            get => _SeFaitDecouper;
            set
            {
                if (value)
                {
                    Entamme = true;
                }
                _SeFaitDecouper = value;
            }
        }
        public Arbre(int x, int y) : base(x, y)
        {
            EstSquatte = false;
            SeFaitDecouper = false;
        }

        public bool estCoupe()
        {
            return Energie == 0;
        }
    }

    public class Camion : CellulleFranchissable
    { 
        public Camion(int x, int y) : base(x, y)
        {
        }
    }

    public class Obstacle : CellulleBloquante
    {
        public Obstacle(int x, int y) : base(x, y)
        {

        }
    }
    public class Carte
    {
        public Cellule[,] tableau = null;
        public int MyScore { get; set; }
        public int EnnemyScore { get; set; }
        public int TagLeft { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int NbTour { get; internal set; }
        public List<Cellule> ListePointsASurveiller = new List<Cellule>();

        public int NbTourMax = 200;

        public bool UtiliserArbreTag = false;

        public PlayableBucheron Curiosity = null;
        public PlayableBucheron _Opportunity = null;
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

        public bool EstACote(int XOrigine, int YOrigine, int XDestination, int YDestination)
        {
            bool ACote = false;

            if (XOrigine - 1 >= 0 && XDestination == (XOrigine - 1) && YDestination == YOrigine)
            {
                ACote = true;
            }
            if (XOrigine + 1 <Width && XDestination == (XOrigine + 1) && YDestination == YOrigine)
            {
                ACote = true;
            }
            if (YOrigine - 1 >= 0 && XDestination == XOrigine && YDestination == (YOrigine - 1))
            {
                ACote = true;
            }
            if (YOrigine + 1 < Height && XDestination == XOrigine && YDestination == (YOrigine + 1))
            {
                ACote = true;
            }


            return ACote;
        }

        public void MettreAJourBucheron(int id, int playerId, int x, int y, int energy, int axeBonus, int inventory)
        {
            //ce sont les miens
            AbstractBucheron bucheron;
            if (playerId == 0)
            {
                //Initialisation de mes bucherons
                if (Curiosity == null)
                {
                    Curiosity = new Curiosity();

                    Curiosity.Id = id;
                }
                else if (_Opportunity == null)
                {
                    _Opportunity = new Opportunity();
                    _Opportunity.Id = id;
                }

                if (Curiosity.Id == id)
                {
                    bucheron = Curiosity;
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
                if (bucheron == Ennemi1 || bucheron == Ennemi2)  //tester s'ils ont bougé depuis le dernier tour
                {
                    bucheron.PasBouge = false;
                    if (bucheron.PrecedentePosition != null && bucheron._X == bucheron.PrecedentePosition.X && bucheron._Y == bucheron.PrecedentePosition.Y && !( bucheron._X == 29 && bucheron._Y == 7))
                    {
                        bucheron.PasBouge = true;
                        Console.Error.WriteLine("PAS BOUGE : " + bucheron._X + "," + bucheron._Y);
                    }
                    bucheron.PrecedentePosition = new CelluleInconnue(x,y);
                }

                bucheron._X = x;
                bucheron._Y = y;
                bucheron.Energy = energy;
                bucheron.BonusHache = axeBonus;
                bucheron.inventory = inventory;
                if (bucheron.CelluleOrigine == null)
                {
                    bucheron.CelluleOrigine = new CelluleInconnue(x, y);
                }
                if (bucheron._X == bucheron.CelluleOrigine.X && bucheron._Y == bucheron.CelluleOrigine.Y)
                {
                    bucheron.Immobile++;
                }
                else
                {
                    bucheron.Immobile = 0;
                }
            }
        }

        public  void MettreAJourTableau(int entityType, int x, int y, int amount)
        {
            Cellule cell = tableau[x, y];
            switch (entityType)
            {
                case 2: //arbre
                    if (cell == null)
                    {
                        Console.Error.WriteLine("Arbre a  x:" + x + " y:" + y);
                        cell = new Arbre(x,y);
                    }
                    if (((Arbre)cell).Energie > amount)
                    {
                        Console.Error.WriteLine("un arbre se fait couper en x:" + x + " y:" + y);
                        ((Arbre)cell).SeFaitDecouper = true;
                    }
                    else
                    {
                        ((Arbre)cell).SeFaitDecouper = false;
                    }
                        ((Arbre)cell).Energie = amount;
                    break;
                case 3: //souche
                    cell = new Obstacle(x,y);
                    break;
                case 4: //obstacle
                    if (cell == null)
                    {
                        cell = new Obstacle(x,y);
                    }
                    break;
                case 5: //caisse de champignons
                    if (cell == null)
                    {
                        cell = new CaisseChampignons(x, y);
                        ((CaisseChampignons)cell).Energie = 100;
                    }
                    break;
                case 7: //Bonus Energie
                    if (cell == null)
                    {
                        cell = new BonusEnergie(x,y);
                        ((BonusEnergie)cell).Energie = amount;
                    }
                    ((BonusEnergie)cell).Visible = true;
                    Console.Error.WriteLine("Je mets à jour un Bonus d'energie " + x + "," + y);
                    break;
                case 8: //Bonus Coupe
                    if (cell == null)
                    {
                        cell = new BonusCoupe(x,y);
                        ((BonusCoupe)cell).AxeBonus = amount;
                    }
                    ((BonusCoupe)cell).Visible = true;
                    break;
                default:
                    throw new Exception("EntityType inconnu : " + entityType + " x:" + x + " y:" + y);
            }
            tableau[x, y] = cell;
            if (tableau[x, y] == null)
            {
                throw new Exception("le tableau ne s'est pas mis à jour !!!");
            }
        }

        public  void InitialiserTableau(int width, int height)
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
            if (width == 30 && height == 15)
            {
                //camion
                tableau[0, 7] = new Camion(0,7);
                //caisse
                tableau[29, 7] = new CaisseChampignons(29,7);

            }
        }

        public void Nettoyer()
        {
            Width = 0;
            Height = 0;
            tableau = null;
            Ennemi1 = null;
            Ennemi2 = null;
            Curiosity = null;
            _Opportunity = null;
            MyScore = 0;
            EnnemyScore = 0;
            TagLeft = 0;
    }

        internal void MettreVisibiliteBonusFalse()
        {
            foreach(Cellule cellule in tableau)
            {
                if (cellule != null && typeof(Bonus).IsAssignableFrom(cellule.GetType()) && !(cellule.X == 29 && cellule.Y == 7))
                {
                    ((Bonus)cellule).Visible = false;
                }
            }
        }

        internal void GererSecuriteTags()
        {
            int Epsilon1 = 100;
            int Epsilon2 = 50;
            bool TagExiste = false;
            bool QueDuRisque = true;
            foreach (Cellule cellule in tableau)
            {
                if (cellule != null && cellule is Arbre)
                {
                    if (((Arbre)cellule).Tag)
                    {
                        TagExiste = true;
                    }
                    if (((Arbre)cellule).Risque == 0 && !((Arbre)cellule).Tag)
                    {
                        QueDuRisque = false;
                    }
                }
            }
            
            if (QueDuRisque)
            {
                UtiliserArbreTag = true;
            }
            else
            {
                UtiliserArbreTag = false;
            }

            if (!TagExiste && NbTour > Epsilon1)
            {
                UtiliserArbreTag = true;
            }

            if (NbTourMax - NbTour <= Epsilon2)
            {
                UtiliserArbreTag = true;
            }

            if (Ennemi1 != null && Ennemi2 != null && Ennemi1.Immobile >= 7 && Ennemi2.Immobile >= 7)
            {
                UtiliserArbreTag = true;
            }
        }

        internal void GererRisque(AbstractBucheron bucheron)
        {
            List<Cellule> listeVoisins = new List<Cellule>();
            if (bucheron.PrecedentePosition.X - 1 >= 0)
            {
                Cellule voisinGauche = new CelluleInconnue(bucheron.PrecedentePosition.X - 1, bucheron.PrecedentePosition.Y);
                listeVoisins.Add(voisinGauche);
            }
            if (bucheron.PrecedentePosition.X + 1 < Width)
            {
                Cellule voisinDroit = new CelluleInconnue(bucheron.PrecedentePosition.X + 1, bucheron.PrecedentePosition.Y);
                listeVoisins.Add(voisinDroit);
            }
            if (bucheron.PrecedentePosition.Y - 1 >= 0)
            {
                Cellule voisinHaut = new CelluleInconnue(bucheron.PrecedentePosition.X, bucheron.PrecedentePosition.Y - 1);
                listeVoisins.Add(voisinHaut);
            }
            if (bucheron.PrecedentePosition.Y + 1 < Height)
            {
                Cellule voisinBas = new CelluleInconnue(bucheron.PrecedentePosition.X, bucheron.PrecedentePosition.Y + 1);
                listeVoisins.Add(voisinBas);
            }

            foreach(Cellule voisin in listeVoisins)
            {
                Cellule celluleTestee = tableau[voisin.X, voisin.Y];
                if (celluleTestee != null && celluleTestee is Arbre)
                {
                    if (!((Arbre)celluleTestee).Entamme)
                    {
                        ((Arbre)celluleTestee).Risque = 1000;
                    }
                }
                if (celluleTestee == null && ListePointsASurveiller.Where(v => v.X == voisin.X && v.Y == voisin.Y).Count() == 0)
                {
                    ListePointsASurveiller.Add(voisin);
                }
            }
        }
    }

    public abstract class AbstractBucheron
    {
        public int Energy { get; set; }
        public int Id { get; set; }
        public int _X { get; set; }
        public int _Y { get; set; }
        public int BonusHache { get; set; }
        public int inventory { get; set; }
        public int Immobile { get; internal set; }
        public Cellule CelluleOrigine { get; internal set; }
        public CelluleInconnue PrecedentePosition { get; internal set; }
        public bool PasBouge { get; internal set; }

        /*private int NombreDeToursPourRetournerAuCamionDepuis(int X, int Y)
        {
            return NombreDeToursPourAllerDepuisEn(X, Y, 0, 7);
        }

        private int NombreDeToursPourAllerEn(int x, int y)
        {
            return NombreDeToursPourAllerDepuisEn(_X, _Y, x, y);
        }*/

        /*public int NombreDeToursPourAllerDepuisEn(int XOrigine, int YOrigine, int XDestination, int YDestination, bool speed = true)
        {
            bool franchissable = Carte.GetInstance().tableau[XDestination, YDestination].Franchissable;
            int NombreDeToursBrut = PathFinder.Solve(XOrigine, YOrigine, XDestination, YDestination, franchissable).Count();
            if (speed)
            {
                return NombreDeToursBrut / 4;
            }
            else
            {
                return NombreDeToursBrut / 2;
            }
        }*/
    }

    public class Bucheron : AbstractBucheron
    {
        public Bucheron()
        {
            Console.Error.WriteLine("Creation d'un Bucheron ennemi");
        }
    }

    public abstract class PlayableBucheron : AbstractBucheron
    {
        protected List<GestionAction> ListeDesActions = new List<GestionAction>();
        public bool JeCoupe { get; set; }
        public bool JeVole { get; set; }
        public bool BesoinRenforts { get; internal set; }

        public Mode _mode, _modeDefaut;

        public Cellule cible = null;

        public String nom = String.Empty;

        public List<Tuple<AbstractBucheron, int>> Followers = new List<Tuple<AbstractBucheron, int>>();

        public Cellule AnciennePosition = null;

        public void AppelerRenforts()
        {
            BesoinRenforts = true;
        }
        /*private bool JeVoisUnBonusDeHache()
        {
            bool BonusExiste = false;
            for (int i = _X - 5; i < _X + 5; i++)
            {
                for (int j = _Y - 5; j < _Y + 5; j++)
                {
                    if (i > 0 && i < Carte.GetInstance().Width && j > 0 && j < Carte.GetInstance().Height)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (cellule is BonusCoupe)
                        {
                            cible = cellule;
                            return true;
                        }
                    }
                }
            }
            return BonusExiste;
        }
        internal void VerifierBonus()
        {
            for (int i = 0; i < Carte.GetInstance().Width; i++)
            {
                for (int j = 0; j < Carte.GetInstance().Height; j++)
                {
                    Cellule cellule = Carte.GetInstance().tableau[i, j];
                    if (cellule != null)
                    {
                        int Distance = Math.Abs(_X - cellule.X) + Math.Abs(_Y - cellule.Y);
                        if ((cellule is BonusCoupe || cellule is BonusEnergie) && !(cellule is CaisseChampignons) && !((Bonus)cellule).Visible == false)
                        {
                            Console.Error.WriteLine(nom + ": je supprime un Bonus " + i + "," + j);
                            Carte.GetInstance().tableau[i, j] = null;
                        }
                    }
                }
            }
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
                            cible = cellule;
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
                            cible = cellule;
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
            for (int i = _X - 5; i < _X + 5; i++)
            {
                for (int j = _Y - 5; j < _Y + 5; j++)
                {
                    if (i >= 0 && i < Carte.GetInstance().Width && j >= 0 && j < Carte.GetInstance().Height)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        AbstractBucheron Ennemi1 = Carte.GetInstance().Ennemi1;
                        AbstractBucheron Ennemi2 = Carte.GetInstance().Ennemi2;
                        if (cellule is Arbre && (Carte.GetInstance().EstACote(Ennemi1._X, Ennemi1._Y, i, j) || Carte.GetInstance().EstACote(Ennemi2._X, Ennemi2._Y, i, j)) && ((Arbre)cellule).Energie <= 4 && ((Arbre)cellule).SeFaitDecouper && !((Arbre)cellule).EstSquatte)
                        {
                            ((Arbre)cellule).EstSquatte = true;
                            cible = cellule;
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
            return Carte.GetInstance().EstACote(_X, _Y, cible.X, cible.Y);
        }

        private bool PeutVoler(int[] cible)
        {
            int Distance = Math.Abs(cible[0] - _X) + Math.Abs(cible[1] - _Y);
            if (Distance <= 4)
            {
                return true;
            }
            return false;
        }*/

        public string DefinirAction()
        {
            if (cible != null)
            {
                Console.Error.WriteLine(nom +  ": cible -> " + cible.X + "," + cible.Y);
                cible = Carte.GetInstance().tableau[cible.X, cible.Y];
                if (cible != null && cible is Obstacle)
                {
                    cible = null;
                }
            }
            if (cible == null)
            {
                JeCoupe = false;
                JeVole = false;
            }

            List<Cellule> ListeSansRisque = new List<Cellule>();
            foreach (Cellule celluleRisque in Carte.GetInstance().ListePointsASurveiller)
            {
                Cellule cellule = Carte.GetInstance().tableau[celluleRisque.X, celluleRisque.Y];
                if (cellule == null) //soit je ne suis pas à distance, soit je ne sais pas
                {
                    if (Math.Abs(_X - celluleRisque.X) + Math.Abs(_Y - celluleRisque.Y) <= 5)//si je suis à distance ==> il n'y a rien
                    {
                        ListeSansRisque.Add(celluleRisque);
                    }
                    else
                    {
                        //je ne peux pas me prononcer => je la garde dans ma liste
                    }
                }
                else
                {
                    if (cellule is Arbre)
                    {
                        ((Arbre)cellule).Risque = 1000;
                    }
                    else
                    {
                        ListeSansRisque.Add(celluleRisque);
                    }
                }
            }

            foreach(Cellule cellule in ListeSansRisque)
            {
                Carte.GetInstance().ListePointsASurveiller.Remove(cellule);
                Console.Error.WriteLine(nom + ": RETIRE " + cellule.X + "," + cellule.Y);
            }
            Console.Error.WriteLine("Points a surveiller : ");
            foreach (Cellule cellule in Carte.GetInstance().ListePointsASurveiller)
            {
                Console.Error.WriteLine(" ==> " + cellule.X + "," + cellule.Y);
            }

            foreach (GestionAction gestion in ListeDesActions)
            {
                String Denomination = gestion.Denomination;
                //Console.Error.WriteLine(nom + ": " + Denomination);
                String action = gestion.DefinirAction();
                if (action != String.Empty)
                {
                    return action;
                }
            }
            throw new Exception("aucune decision n'a ete prise");
        }

        internal void VerifierBonus()
        {
            for (int i = (_X - 5); i <= (_X + 5); i++)
            {
                for (int j = (_Y - 5); j <= (_Y + 5); j++)
                {
                    if (i >= 0 && j >= 0 && i < Carte.GetInstance().Width && j < Carte.GetInstance().Height && Math.Abs(_X - i) + Math.Abs(_Y - j) <= 5)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (cellule != null && typeof(Bonus).IsAssignableFrom(cellule.GetType()) && !((Bonus)cellule).Visible)
                        {
                            if (cellule.X == 29 && cellule.Y == 7)
                            {

                            }
                            else
                            {
                                Console.Error.WriteLine(nom + " Destruction du Bonus : " + cellule.X + "," + cellule.Y);
                                Carte.GetInstance().tableau[i, j] = null;
                            }
                        }
                    }
                }
            }
        }
    }

    public class Curiosity : PlayableBucheron
    {
        public Curiosity()
        {
            Console.Error.WriteLine("Hello Curiosity");
            _mode = Mode.MODE_OPPORTUNISTE;
            _modeDefaut = Mode.MODE_OPPORTUNISTE;
            nom = "Curiosity";

            //ajout des opérations dans le "cerveau"
            ListeDesActions.Add(new GestionActionInventaire(this));
            ListeDesActions.Add(new GestionActionEnergie(this));
            ListeDesActions.Add(new GestionActionVol(this));
            ListeDesActions.Add(new GestionActionTag(this));
            ListeDesActions.Add(new GestionActionBonusHache(this));
            ListeDesActions.Add(new GestionActionBucheron(this));
            ListeDesActions.Add(new GestionActionExploration(this));
        }
    }

    public class Opportunity : PlayableBucheron
    {

        public Opportunity()
        {
            Console.Error.WriteLine("Hello Opportunity");
            _mode = Mode.MODE_OPPORTUNISTE;
            _modeDefaut = Mode.MODE_OPPORTUNISTE;
            nom = "Opportunity";

            //ajout des opérations dans le "cerveau"
            ListeDesActions.Add(new GestionActionInventaire(this));
            ListeDesActions.Add(new GestionActionEnergie(this));
            ListeDesActions.Add(new GestionActionBonusHache(this));
            ListeDesActions.Add(new GestionActionBucheron(this));
            ListeDesActions.Add(new GestionActionExploration(this));
        }
    }

    public abstract class GestionAction
    {
        public String Denomination;
        abstract public String DefinirAction();

        protected PlayableBucheron _bucheron;

        public GestionAction(PlayableBucheron bucheron)
        {
            _bucheron = bucheron;
        }

        public String PathFinding_Cible()
        {
            String action = PathFinding(_bucheron.cible.X, _bucheron.cible.Y);
            if (action.Equals(String.Empty))
            {
                action = "WAIT"; //UGLY !!!
                Carte.GetInstance().tableau[_bucheron.cible.X, _bucheron.cible.Y].Bloque = true;
                _bucheron.cible = null;
            }
            return action;
        }

        /// <summary>
        /// renvoie une chaine de caractères correspondant à l'action à effectuer pour aller au prochain point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        protected String PathFinding(int x, int y)
        {
            bool franchissable = Carte.GetInstance().tableau[x, y].Franchissable;
            List<Noeud> Chemin = PathFinder.Solve(_bucheron._X, _bucheron._Y, x, y, franchissable);
            if (Chemin == null)
            {
                return String.Empty;
            }
            if (Chemin.Count == 0)
            {
                return "MOVE 0 7";
                //throw new Exception("pathfinding sur moi meme");
            }
            int pas = Math.Min(2, Chemin.Count()) - 1;
            if (_bucheron.Energy > 0 && _bucheron.Energy <= 1)
            {

                pas = Math.Min(3, Chemin.Count()) - 1;
            }
            if (_bucheron.Energy >= 2)
            {
                pas = Math.Min(4, Chemin.Count()) - 1;
            }
            return "MOVE " + Chemin[pas].x + " " + Chemin[pas].y;
        }
        public String PathFinding_Camion()
        {
            Console.Error.WriteLine("PathFinding Camion");
            return PathFinding(0, 7);
        }

        protected String PathFinding_CaisseChampignon()
        {
            return PathFinding(29, 7);
        }

        public Cellule JeTrouveArbreTaggeLePlusProche()
        {
            List<Tuple<Arbre, int>> listeArbres = new List<Tuple<Arbre, int>>();
            foreach (Cellule cellule in Carte.GetInstance().tableau)
            {
                if (cellule != null && cellule is Arbre && ((Arbre)cellule).Tag)
                {
                    listeArbres.Add(new Tuple<Arbre, int>(((Arbre)cellule), Math.Abs(_bucheron._X - cellule.X) + Math.Abs(_bucheron._Y - cellule.Y)));
                }
            }
            if (listeArbres.Count() > 0)
            {
                int min = listeArbres.Min(p => p.Item2);
                return listeArbres.Where(a => a.Item2 == min).First().Item1;
            }
            return null;
        }

        public void JeTrouveLePlusRentable(Type type)
        {
            if (Type.Equals(type, typeof(Arbre)))
            {
                List<Tuple<Cellule, int>> ListeDesPositions = new List<Tuple<Cellule, int>>();
                for (int i = 0; i < Carte.GetInstance().Width; i++)
                {
                    for (int j = 0; j < Carte.GetInstance().Height; j++)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (Carte.GetInstance().UtiliserArbreTag)
                        {
                            if (cellule != null && !cellule.Bloque && cellule is Arbre && !((Arbre)cellule).SeFaitDecouper)
                            {
                                int FacteurHache = 1;
                                if (_bucheron.BonusHache > 0)
                                {
                                    FacteurHache = 2;
                                }
                                int poids = ((Arbre)cellule).Risque +
                                    (Math.Abs(_bucheron._X - i) + Math.Abs(_bucheron._Y - j) / 2) + //Nb de Tours du bucheron a l'arbre
                                    (
                                        (int)Math.Ceiling((double)((Arbre)Carte.GetInstance().tableau[i, j]).Energie / FacteurHache) // Temps que je mets à le couper 
                                    ) +
                                    (Math.Abs(0 - i) + Math.Abs(7 - j) / 2); // Nb de Tours de l'arbre au camion
                                ListeDesPositions.Add(new Tuple<Cellule, int>(cellule, poids));
                                Console.Error.WriteLine(_bucheron.nom + ": " + cellule.X + "," + cellule.Y + " -> " + poids);
                            }
                        }
                        else
                        {
                            if (cellule != null && !cellule.Bloque && cellule is Arbre && !((Arbre)cellule).SeFaitDecouper && !((Arbre)cellule).Tag)
                            {
                                int FacteurHache = 1;
                                if (_bucheron.BonusHache > 0)
                                {
                                    FacteurHache = 2;
                                }
                                int poids = ((Arbre)cellule).Risque +
                                    (Math.Abs(_bucheron._X - i) + Math.Abs(_bucheron._Y - j) / 2) + //Nb de Tours du bucheron a l'arbre
                                    (
                                        (int)Math.Ceiling((double)((Arbre)Carte.GetInstance().tableau[i, j]).Energie / FacteurHache) // Temps que je mets à le couper 
                                    ) +
                                    (Math.Abs(0 - i) + Math.Abs(7 - j) / 2); // Nb de Tours de l'arbre au camion
                                ListeDesPositions.Add(new Tuple<Cellule, int>(cellule, poids));
                                Console.Error.WriteLine(_bucheron.nom + ": " + cellule.X + "," + cellule.Y + " -> " + poids);
                            }
                        }
                    }
                }
                Tuple<Cellule, int> arbre = null;
                if (ListeDesPositions.Count > 0)
                {
                    int NbToursMin = ListeDesPositions.Select(p => p.Item2).Min();
                    List<Tuple<Cellule, int>> ListeDesMeilleurs = ListeDesPositions.Where(p => p.Item2 == NbToursMin).ToList();
                    foreach(Tuple<Cellule, int> position in ListeDesMeilleurs)
                    {
                        if (_bucheron.cible != null && _bucheron.cible.X == position.Item1.X && _bucheron.cible.Y == position.Item1.Y)
                        {
                            _bucheron.cible = Carte.GetInstance().tableau[position.Item1.X, position.Item1.Y];
                            return;
                        }
                    }
                    arbre = ListeDesPositions.Where(p => p.Item2 == NbToursMin).FirstOrDefault();
                }


                if (arbre == null)
                {
                    Console.Error.WriteLine(_bucheron.nom + ": je n'ai pas trouvé d'arbre");
                    _bucheron.cible = null;
                }
                else
                {
                    _bucheron.cible = Carte.GetInstance().tableau[arbre.Item1.X, arbre.Item1.Y];
                }
            }
            else if (Type.Equals(type, typeof(BonusEnergie)))
            {
                List<Tuple<Cellule, float>> ListeDesPositions = new List<Tuple<Cellule, float>>();
                for (int i = 0; i < Carte.GetInstance().Width; i++)
                {
                    for (int j = 0; j < Carte.GetInstance().Height; j++)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];

                        if (cellule != null && !cellule.Bloque && (cellule is BonusEnergie || cellule is CaisseChampignons))
                        {
                            int NbTours = (int)Math.Ceiling((float)(Math.Abs(_bucheron._X - i) + Math.Abs(_bucheron._Y - j)) / 4);
                            if (NbTours > 0)
                            {
                                ListeDesPositions.Add(new Tuple<Cellule,float>(cellule, CalculPoidsEnergie((BonusEnergie)cellule) ));
                            }
                        }
                    }
                }

                foreach(Tuple<Cellule, float> position in ListeDesPositions)
                {
                    Console.Error.WriteLine("-->" + position.Item1.X + "," + position.Item1.Y + " : " + position.Item2);
                }

                if (ListeDesPositions.Count > 0)
                {
                    float PoidsMax = ListeDesPositions.Select(p => p.Item2).Max();
                    float PoidsMin = ListeDesPositions.Select(p => p.Item2).Min();
                    foreach (Tuple<Cellule, float> P in ListeDesPositions.Where(p=> p.Item2 == PoidsMax))
                    {
                        if (_bucheron.cible != null && P.Item1.X == _bucheron.cible.X && P.Item1.Y == _bucheron.cible.Y)
                        {
                            return;
                        }
                    }
                    Tuple<Cellule, float> pire = ListeDesPositions.Where(p => p.Item2 == PoidsMin).FirstOrDefault();
                    if (_bucheron.cible != null && _bucheron.cible is BonusEnergie && !_bucheron.cible.Equals(pire)) //Evite la danse ?
                    {
                        return;
                    }
                    Tuple<Cellule, float> position = ListeDesPositions.Where(p => p.Item2== PoidsMax).FirstOrDefault();
                    _bucheron.cible = Carte.GetInstance().tableau[position.Item1.X, position.Item1.Y];
                }
                else
                {
                    return;
                }
            }
            else
            {
                throw new Exception("Type non Connu : " + type);
            }
        }
        protected float CalculPoidsEnergie(BonusEnergie cellule)
        {
            int Distance = Math.Abs(_bucheron._X - cellule.X) + Math.Abs(_bucheron._Y - cellule.Y);
            int NbTours = (int)Math.Floor((float)Distance / 2) + 1;
            return ((float)cellule.Energie / (float)NbTours);
        }

        public bool IlExistePasLoin(Type type)
        {
            bool existe = false;
            for (int i = _bucheron._X - 5; i <= _bucheron._X + 5; i++)
            {
                for (int j = _bucheron._Y - 5; j <= _bucheron._Y + 5; j++)
                {
                    if (i >= 0 && i < Carte.GetInstance().Width && j >= 0 && j < Carte.GetInstance().Height)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (cellule != null && ! cellule.Bloque)
                        {
                            if (type.IsAssignableFrom(cellule.GetType()))
                            {
                                existe = true;
                            }
                        }
                    }
                }
            }
            return existe;
        }

        public void TrouverEnVisu(Type type)
        {
            List<Tuple<Cellule,float>> listeObjets = new List<Tuple<Cellule, float>>();
            for (int i = _bucheron._X -5; i <= _bucheron._X +5; i++)
            {
                for (int j = _bucheron._Y - 5; j <= _bucheron._Y + 5; j++)
                {
                    if (i >= 0 && i < Carte.GetInstance().Width && j >= 0 && j < Carte.GetInstance().Height)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (cellule != null && ! cellule.Bloque)
                        {
                            if (type.IsAssignableFrom(cellule.GetType()))
                            {
                                if (Type.Equals(type, typeof(BonusEnergie)))
                                {
                                    listeObjets.Add(new Tuple<Cellule, float>(cellule, CalculPoidsEnergie((BonusEnergie)cellule)));
                                }
                                else if (Type.Equals(type, typeof(BonusCoupe)))
                                {
                                    listeObjets.Add(new Tuple<Cellule, float>(cellule, ((BonusCoupe)cellule).AxeBonus));
                                }
                                else
                                {
                                    throw new Exception("Type non gere : " + type);
                                }
                            }
                        }
                    }
                }
            }
            listeObjets = listeObjets.OrderBy(e => e.Item2).ToList();
            Tuple<Cellule, float> meilleur = listeObjets.LastOrDefault();
            Tuple<Cellule, float> pire = listeObjets.FirstOrDefault();
            if (_bucheron.cible != null && type.IsAssignableFrom(_bucheron.cible.GetType()) &&  !_bucheron.cible.Equals(pire)) //Evite la danse ?
            {
                return; 
            }
            if (meilleur != null)
            {
                _bucheron.cible = meilleur.Item1;
            }
            else
            {
                _bucheron.cible = null;
            }
        }
    }

    public class GestionActionEnergie : GestionAction
    {
        public GestionActionEnergie(PlayableBucheron bucheron) : base(bucheron)
        {
            Denomination = "GestionActionEnergie";
        }
        public override string DefinirAction()
        {
            int Epsilon1 = 15;
            int Epsilon2 = 30;
            String action = String.Empty;
            if (!_bucheron.JeCoupe && !_bucheron.JeVole && _bucheron.Energy > 0 && _bucheron.Energy <= Epsilon1 && IlExistePasLoin(typeof(BonusEnergie)))//si je ne porte rien et je ne coupe pas, ne vole pas, que j'ai moins de Epsilon energie et que j'ai un Bonus Energie à moins de 5 cases
            {
                TrouverEnVisu(typeof(BonusEnergie));
                if (_bucheron.cible == null)
                {
                    throw new Exception("je n'ai pas de cible 1");
                }
                action = PathFinding_Cible();
            }
            if (_bucheron.Energy <= 50 && !_bucheron.JeVole && ! _bucheron.JeCoupe && Carte.GetInstance().NbTourMax - Carte.GetInstance().NbTour >= Epsilon2)
            {
                JeTrouveLePlusRentable(typeof(BonusEnergie));
                if (_bucheron.cible == null)
                {
                    throw new Exception("je n'ai pas de cible 2");
                }
                action = PathFinding_Cible();
            }
            return action;
        }
    }

    public class GestionActionInventaire : GestionAction
    {
        public GestionActionInventaire(PlayableBucheron bucheron) : base(bucheron)
        {
            Denomination = "GestionActionInventaire";
        }
        public override string DefinirAction()
        {
            String action = String.Empty;

            if ( _bucheron.inventory > 0)
            {
                _bucheron.JeCoupe = false;
                _bucheron.JeVole = false;
                action = PathFinding_Camion();
            }

            return action;
        }
    }

    public class GestionActionBucheron : GestionAction
    {
        public GestionActionBucheron(PlayableBucheron bucheron) : base(bucheron)
        {
            Denomination = "GestionActionBucheron";
        }
        public override string DefinirAction()
        {
            String action = String.Empty;

            if (_bucheron.JeCoupe)
            {
                if (Carte.GetInstance().tableau[_bucheron.cible.X, _bucheron.cible.Y] == null)
                { throw new Exception("FUCK !"); }
                if (Carte.GetInstance().tableau[_bucheron.cible.X,_bucheron.cible.Y] is Obstacle)
                {
                    _bucheron.JeCoupe = false;
                }
                else
                {
                    _bucheron.JeCoupe = true;
                    action = "CUT " + _bucheron.cible.X + " " + _bucheron.cible.Y;
                    ((Arbre)_bucheron.cible).SeFaitDecouper = true;
                    return action;
                }
            }
            Cellule cellule = null;
            bool suivi = JeSuisSuivi();
            if (suivi) //Spike killer
            {
                cellule = JeTrouveArbreTaggeLePlusProche();
            }
            if (cellule == null)
            {
                JeTrouveLePlusRentable(typeof(Arbre));
            }
            else
            {
                _bucheron.cible = cellule;
            }
            if (suivi && PasEnnemiSurMaCase())
            {
                action = "WAIT";
            }
            if (_bucheron.cible != null)
            {
                if (Carte.GetInstance().EstACote(_bucheron._X, _bucheron._Y, _bucheron.cible.X, _bucheron.cible.Y))
                {
                    _bucheron.JeCoupe = true;
                    action = "CUT " + _bucheron.cible.X + " " + _bucheron.cible.Y;
                    ((Arbre)_bucheron.cible).SeFaitDecouper = true;
                }
                else
                {
                    action = PathFinding_Cible();
                }
            }
            return action;
        }

        private bool PasEnnemiSurMaCase()
        {
            bool OK = false;

            if (Carte.GetInstance().Ennemi1._X == _bucheron._X && Carte.GetInstance().Ennemi1._Y == _bucheron._Y)
            {
                OK = true;
            }

            return OK;
        }
        private bool JeSuisSuivi()
        {
            bool suivi = false;
            int Epsilon = 2;
            if (_bucheron.AnciennePosition == null)
            {
                return false;
            }

            if (_bucheron.JeVole || _bucheron.JeVole)
            {
                _bucheron.Followers = new List<Tuple<AbstractBucheron, int>>();
            }

            AbstractBucheron Ennemi1 = Carte.GetInstance().Ennemi1;
            AbstractBucheron Ennemi2 = Carte.GetInstance().Ennemi2;
            if (Ennemi1._X == _bucheron.AnciennePosition.X && Ennemi1._Y == _bucheron.AnciennePosition.Y)
            {
                Console.Error.WriteLine(_bucheron.nom + ": on marche sur mes pas");
                Tuple<AbstractBucheron, int> tuple;
                int compteur = 0;
                if (_bucheron.Followers.Where(f => f.Item1 == Ennemi1).Count() > 0)
                {
                    tuple = _bucheron.Followers.Where(f => f.Item1 == Ennemi1).First();
                    _bucheron.Followers.Remove(tuple);
                    compteur = tuple.Item2;
                }
                tuple = new Tuple<AbstractBucheron, int>(Ennemi1, compteur + 1);
                _bucheron.Followers.Add(tuple);
            }
            else
            {
                if (_bucheron.Followers.Where(f => f.Item1 == Ennemi1).Count() > 0)
                {
                    Tuple<AbstractBucheron, int> tuple = _bucheron.Followers.Where(f => f.Item1 == Ennemi1).First();
                    if (tuple != null)
                    {
                        _bucheron.Followers.Remove(tuple);
                    }
                }
            }

            if (Ennemi2._X == _bucheron.AnciennePosition.X && Ennemi2._Y == _bucheron.AnciennePosition.Y)
            {

                Console.Error.WriteLine(_bucheron.nom + ": on marche sur mes pas");
                Tuple<AbstractBucheron, int> tuple;
                int compteur = 0;
                if (_bucheron.Followers.Where(f => f.Item1 == Ennemi2).Count() > 0)
                {
                    tuple = _bucheron.Followers.Where(f => f.Item1 == Ennemi2).First();
                    _bucheron.Followers.Remove(tuple);
                    compteur = tuple.Item2;
                }
                tuple = new Tuple<AbstractBucheron, int>(Ennemi2, compteur + 1);
                _bucheron.Followers.Add(tuple);
            }
            else
            {
                if (_bucheron.Followers.Where(f => f.Item1 == Ennemi2).Count() > 0)
                {
                    Tuple<AbstractBucheron, int> tuple = _bucheron.Followers.Where(f => f.Item1 == Ennemi2).First();
                    _bucheron.Followers.Remove(tuple);
                }
            }

            if (_bucheron.Followers.Where(f => f.Item2 >= Epsilon).Count() > 0)
            {
                Console.Error.WriteLine(_bucheron.nom + ": je suis suivi");
                suivi = true;
            }

            return suivi;
        }
    }

    public class GestionActionTag : GestionAction
    {
        public GestionActionTag(PlayableBucheron bucheron) : base(bucheron)
        {
            Denomination = "GestionActionTag";
        }
        public override string DefinirAction()
        {
            String action = String.Empty;

            if (Carte.GetInstance().TagLeft > 0)
            {
                JeTrouveArbreLePlusRentablePourTag();
                if (_bucheron.cible != null)
                {
                    if (Carte.GetInstance().EstACote(_bucheron._X,_bucheron._Y,_bucheron.cible.X,_bucheron.cible.Y))
                    {
                        ((Arbre)_bucheron.cible).Tag = true;
                        action = "TAG " + _bucheron.cible.X + " " + _bucheron.cible.Y;
                    }
                    else
                    {
                        action = PathFinding_Cible();
                    }
                }
            }
            return action;
        }

        private void JeTrouveArbreLePlusRentablePourTag()
        {
            List<Tuple<Cellule, float>> ListeTuple = new List<Tuple<Cellule, float>>();
            foreach(Cellule cellule in Carte.GetInstance().tableau)
            {
                if (cellule != null && cellule is Arbre && !cellule.Bloque && !((Arbre)cellule).Tag && !((Arbre)cellule).Entamme)
                {
                    ListeTuple.Add(new Tuple<Cellule, float>(cellule, CalculPerturbationArbre((Arbre)cellule)));
                }
            }
            if (ListeTuple.Count > 0)
            {
                float min = ListeTuple.Min(p => p.Item2);
                Tuple<Cellule, float> piege = ListeTuple.Where(t => t.Item2 == min).FirstOrDefault();
                if (piege != null)
                {
                    _bucheron.cible = piege.Item1;
                }
            }
        }

        private float CalculPerturbationArbre(Arbre cellule)
        {
            float poids = 0;

            poids = cellule.Risque + Convert.ToSingle(2 * (Math.Floor((double)(Math.Abs(cellule.X - 0) + Math.Abs(cellule.Y - 7))/(double)4)) + cellule.Energie);

            return poids;
        }
    }

    public class GestionActionBonusHache : GestionAction
    {
        public GestionActionBonusHache(PlayableBucheron bucheron) : base(bucheron)
        {
            Denomination = "GestionActionBonusHache";
        }
        public override string DefinirAction()
        {
            String action = String.Empty;

            if (_bucheron.BonusHache <= 1 && !_bucheron.JeCoupe && ! _bucheron.JeVole && IlExistePasLoin(typeof(BonusCoupe)))
            {
                TrouverEnVisu(typeof(BonusCoupe));
                if (_bucheron.cible == null)
                {
                    throw new Exception("je n'ai pas retouvé le BonusCoupe");
                }
                action = PathFinding_Cible();
            }

            return action;
        }
    }

    public class GestionActionVol : GestionAction
    {
        public GestionActionVol(PlayableBucheron bucheron) : base(bucheron)
        {
            Denomination = "GestionActionVol";
        }
        public override string DefinirAction()
        {
            String action = String.Empty;
            if (_bucheron.JeVole)
            {
                if (!IlYAUnEnnemiACoteDeMaCible())
                {
                    _bucheron.JeVole = false;
                    _bucheron.cible = null;
                }
                else
                {
                    if (!(_bucheron.cible is Arbre ) || !((Arbre)_bucheron.cible).SeFaitDecouper)
                    {
                        Console.Error.WriteLine("ce FDP m'a grille");
                        _bucheron.JeVole = false;
                        _bucheron.cible = null;
                    }
                    else
                    {
                        action = "WAIT";
                        return action;
                    }
                }
            }
            if (! _bucheron.JeCoupe && JePeuxVolerArbre())
            {
                if (Carte.GetInstance().EstACote(_bucheron._X,_bucheron._Y,_bucheron.cible.X,_bucheron.cible.Y))
                {
                    action = "WAIT";
                    _bucheron.JeVole = true;
                    _bucheron.JeCoupe = false;
                }
                else
                {
                    action = PathFinding_Cible();
                }
            }
            return action;
        }

        private bool JePeuxVolerArbre()
        {
            Cellule cible = _bucheron.cible;
            bool jePeuxVoler = false;
            foreach (Cellule cellule in Carte.GetInstance().tableau)
            {
                if (cellule != null && cellule is Arbre && !((Arbre)cellule).Bloque && ((Arbre)cellule).SeFaitDecouper && ! ((Arbre)cellule).Tag )
                {
                    //je regarde si j'aurai le temps d'y aller
                    int distance = PathFinder.Solve(_bucheron._X, _bucheron._Y, cellule.X, cellule.Y, false).Count();
                    if (Math.Floor((double)distance / (double)4) <= ((Arbre)cellule).Energie && _bucheron.Energy >= 2 * (int)distance / 4 + Math.Max(distance % 4 - 2, 0))
                    {
                        _bucheron.cible = cellule;
                        if (IlYAUnEnnemiACoteDeMaCible())
                        {
                            return true;
                        }
                    }
                }
            }
            _bucheron.cible = cible;
            return jePeuxVoler;
        }

        private bool IlYAUnEnnemiACoteDeMaCible()
        {
            bool present = false;

            AbstractBucheron Ennemi1 = Carte.GetInstance().Ennemi1;
            AbstractBucheron Ennemi2 = Carte.GetInstance().Ennemi2;

            if (Ennemi1._X == _bucheron.cible.X - 1 && Ennemi1._Y == _bucheron.cible.Y)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.cible.X - 1 && Ennemi2._Y == _bucheron.cible.Y)
            {
                present = true;
            }
            if (Ennemi1._X == _bucheron.cible.X + 1 && Ennemi1._Y == _bucheron.cible.Y)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.cible.X + 1 && Ennemi2._Y == _bucheron.cible.Y)
            {
                present = true;
            }
            if (Ennemi1._X == _bucheron.cible.X && Ennemi1._Y == _bucheron.cible.Y - 1)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.cible.X && Ennemi2._Y == _bucheron.cible.Y - 1)
            {
                present = true;
            }
            if (Ennemi1._X == _bucheron.cible.X && Ennemi1._Y == _bucheron.cible.Y + 1)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.cible.X && Ennemi2._Y == _bucheron.cible.Y + 1)
            {
                present = true;
            }

            return present;
        }
    }

    public class GestionActionExploration : GestionAction
    {
        public GestionActionExploration(PlayableBucheron bucheron) : base(bucheron)
        {
            Denomination = "GestionActionExploration";
        }
        public override string DefinirAction()
        {
            String action = String.Empty;
            action = "MOVE 29 7";
            return action;
        }
    }


    public class Player
    {
        static void Main(string[] args)
        {
            bool debug = false;
            List<String> inputLines = new List<string>(); ;
            if (debug)
            {
                inputLines = File.ReadAllLines("debug.txt").ToList();
            }
            String ligne;
            string[] inputs;
            if (debug)
            {
                ligne = inputLines.ElementAt(0);
                inputLines.RemoveAt(0);
            }
            else
            {
                ligne = Console.ReadLine();
            }
            //Console.Error.WriteLine(ligne);
            inputs = ligne.Split(' ');
            int width = int.Parse(inputs[0]);
            int height = int.Parse(inputs[1]);

            if (debug)
            {
                ligne = inputLines.ElementAt(0);
                inputLines.RemoveAt(0);
            }
            else
            {
                ligne = Console.ReadLine();
            }
            int N = int.Parse(ligne);
            Console.Error.WriteLine("N:" + N);
            for (int i = 0; i < N; i++)
            {
                if (debug)
                {
                    ligne = inputLines.ElementAt(0);
                    inputLines.RemoveAt(0);
                }
                else
                {
                    ligne = Console.ReadLine();
                }
                //Console.Error.WriteLine(ligne);

                inputs = ligne.Split(' ');
                int cellType = int.Parse(inputs[0]); // 0 for truck, 1 for energy cells
                int x = int.Parse(inputs[1]); // position of the entity
                int y = int.Parse(inputs[2]); // position of the entity
            }

            //
            Carte carte = Carte.GetInstance();
            carte.InitialiserTableau(width, height);
            //carte.NbTags = N;

            // game loop
            while (true)
            {
                Carte.GetInstance().NbTour++;
                Carte.GetInstance().GererSecuriteTags();
                Carte.GetInstance().MettreVisibiliteBonusFalse();
                if (debug)
                {
                    ligne = inputLines.ElementAt(0);
                    inputLines.RemoveAt(0);
                }
                else
                {
                    ligne = Console.ReadLine();
                }
                //Console.Error.WriteLine("score ==>" +ligne);
                inputs = ligne.Split(' ');
                int myScore = int.Parse(inputs[0]); // Amount of trees delivered
                int opponentScore = int.Parse(inputs[1]);
                int tagsLeft = int.Parse(inputs[2]);
                carte.MyScore = myScore;
                carte.TagLeft = tagsLeft;

                for (int i = 0; i < 4; i++)
                {
                    if (debug)
                    {
                        ligne = inputLines.ElementAt(0);
                        inputLines.RemoveAt(0);
                    }
                    else
                    {
                        ligne = Console.ReadLine();
                    }
                    //Console.Error.WriteLine(ligne);
                    inputs = ligne.Split(' ');
                    int id = int.Parse(inputs[0]); // unique id of the entity
                    int playerId = int.Parse(inputs[1]); // 0 for your lumberjack, 1 for other lumberjack
                    int x = int.Parse(inputs[2]);
                    int y = int.Parse(inputs[3]);
                    int energy = int.Parse(inputs[4]); // energy meter of the lumberjack
                    int axeBonus = int.Parse(inputs[5]); // while > 0, each cut count twice
                    int inventory = int.Parse(inputs[6]); // 0 if empty, else lumberjack carry a tree
                    carte.MettreAJourBucheron(id, playerId, x, y, energy, axeBonus, inventory);
                }
                if (debug)
                {
                    ligne = inputLines.ElementAt(0);
                    inputLines.RemoveAt(0);
                }
                else
                {
                    ligne = Console.ReadLine();
                }
                //Console.Error.WriteLine(ligne);
                int entityCount = int.Parse(ligne); // number of entities visible to you
                for (int i = 0; i < entityCount; i++)
                {
                    if (debug)
                    {
                        ligne = inputLines.ElementAt(0);
                        inputLines.RemoveAt(0);
                    }
                    else
                    {
                        ligne = Console.ReadLine();
                    }
                    //Console.Error.WriteLine(ligne);
                    inputs = ligne.Split(' ');
                    int id = int.Parse(inputs[0]);
                    int entityType = int.Parse(inputs[1]); // 2 for tree, 3 for stump, 4 for fence, 7 for energy bonus, 8 for axe bonus
                    int x = int.Parse(inputs[2]);
                    int y = int.Parse(inputs[3]);
                    int amount = int.Parse(inputs[4]); // depends of type (see rules)
                    carte.MettreAJourTableau(entityType, x, y, amount);
                    if (carte.Ennemi1.PasBouge)
                    {
                        carte.GererRisque(carte.Ennemi1);
                    }
                    if (carte.Ennemi2.PasBouge)
                    {
                        carte.GererRisque(carte.Ennemi2);
                    }
                }
                carte.Curiosity.VerifierBonus();
                carte._Opportunity.VerifierBonus();
                String OpportunityAction =carte._Opportunity.DefinirAction();
                String CuriosityAction = carte.Curiosity.DefinirAction();

                if (((PlayableBucheron) carte.Curiosity).BesoinRenforts || ((PlayableBucheron)carte._Opportunity).BesoinRenforts)
                {
                    if (carte.Curiosity.BesoinRenforts && carte._Opportunity.BesoinRenforts) //on donne la priorité à Opportunity, pas forcément Opti
                    {
                        carte.Curiosity.BesoinRenforts = false;
                    }
                    if (carte.Curiosity.BesoinRenforts)
                    {
                        //String 
                    }
                }
                carte.Curiosity.AnciennePosition = new CelluleInconnue(carte.Curiosity._X, carte.Curiosity._Y);
                carte._Opportunity.AnciennePosition = new CelluleInconnue(carte._Opportunity._X, carte._Opportunity._Y);
                Console.Out.WriteLine(CuriosityAction);
                Console.Out.WriteLine(OpportunityAction);
            }
        }
    }
}