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
        public static List<Noeud> GridSolve(int XOrigine, int YOrigine, int XDestination, int YDestination, int[,]carte)
        {
            List<Noeud> chemin = new List<Noeud>();
            Noeud racine = new Noeud();
            racine.x = XDestination;
            racine.y = YDestination;
            bool done = false;

            //gestion des noeuds bloqués
            if (carte[XDestination, YDestination] == int.MaxValue || ObtenirPoids(XDestination,YDestination,carte) == int.MaxValue)
            {
                return chemin;
            }

            chemin.Add(racine);

            if (XOrigine == XDestination && YOrigine == YDestination)
            {
                return chemin;
            }

            Noeud noeudCourant = racine;
            while (!done)
            {
                int poids;
                if (carte[noeudCourant.x,noeudCourant.y] == -1)
                {
                    poids = int.MaxValue;
                }
                else
                {
                    poids = carte[noeudCourant.x, noeudCourant.y];
                }

                Noeud voisindroit = new Noeud();
                voisindroit.x = noeudCourant.x + 1;
                voisindroit.y = noeudCourant.y;

                Noeud voisinGauche = new Noeud();
                voisinGauche.x = noeudCourant.x - 1;
                voisinGauche.y = noeudCourant.y;

                Noeud voisinHaut = new Noeud();
                voisinHaut.x = noeudCourant.x;
                voisinHaut.y = noeudCourant.y - 1;

                Noeud voisinBas = new Noeud();
                voisinBas.x = noeudCourant.x;
                voisinBas.y = noeudCourant.y + 1;


                if (voisinGauche.x >= 0 && carte[voisinGauche.x, voisinGauche.y] < poids && carte[voisinGauche.x, voisinGauche.y] != -1)
                {
                    chemin.Add(voisinGauche);
                    noeudCourant = voisinGauche;
                }
                else if (voisindroit.x < Carte.GetInstance().Width && carte[voisindroit.x, voisindroit.y] < poids && carte[voisindroit.x, voisindroit.y] != -1)
                {
                    chemin.Add(voisindroit);
                    noeudCourant = voisindroit;
                }
                else if (voisinHaut.y >= 0 && carte[voisinHaut.x, voisinHaut.y] < poids && carte[voisinHaut.x, voisinHaut.y] != -1)
                {
                    chemin.Add(voisinHaut);
                    noeudCourant = voisinHaut;
                }
                else if (voisinBas.y < Carte.GetInstance().Height && carte[voisinBas.x, voisinBas.y] < poids && carte[voisinBas.x, voisinBas.y] != -1)
                {
                    chemin.Add(voisinBas);
                    noeudCourant = voisinBas;
                }
                else
                {
                    foreach (Noeud voisin in chemin)
                    {
                       //Console.Error.WriteLine(voisin.x + "," + voisin.y);
                    }
                    throw new Exception("j'ai merdé ");
                }

                if (noeudCourant.x == XOrigine && noeudCourant.y == YOrigine)
                {
                    done = true;
                }

            }

            if (chemin.Count > 0 && carte[chemin[0].x,chemin[0].y] == -1)
            {
                chemin.RemoveAt(0);
            }

            chemin.Reverse();


            foreach (Noeud voisin in chemin)
            {
               //Console.Error.WriteLine(voisin.x + "," + voisin.y);
            }
           //Console.Error.WriteLine("___________________________");
            return chemin;
        }
        /*public static List<Noeud> AStarSolve(int XOrigine, int YOrigine, int XDestination, int YDestination, bool Franchissable)
        {
            //Console.Error.WriteLine("PathFinding de " + XOrigine + "," + YOrigine + " vers " + XDestination + "," + YDestination + " franchissable " + Franchissable);
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
                    if ((_noeud.x == XDestination && _noeud.y == YDestination) && Franchissable)
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
            while (noeud != null)
            {
                chemin.Insert(0, noeud);
                noeud = noeud.parent;
            }
            if (chemin.Count() > 0)
            {
                chemin.RemoveAt(0);
            }
            return chemin;
        }*/

        /*private static IEnumerable<Noeud> ObtenirVoisins(Noeud noeud)
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
        }*/

        internal static int ObtenirPoids(int x, int y, int[,] carte)
        {
            int retour = int.MaxValue;
            int[,] TableauPoids = carte;
            if (TableauPoids[x,y] == int.MaxValue) //pour gérer les objets inaccessibles
            {
                return int.MaxValue;
            }
            if (x - 1 >= 0 && TableauPoids[x - 1, y] != -1)
            {
                retour = Math.Min(retour, TableauPoids[x - 1, y]);
            }
            if (x + 1 < Carte.GetInstance().Width && TableauPoids[x + 1, y] != -1)
            {
                retour = Math.Min(retour, TableauPoids[x + 1, y]);
            }
            if (y - 1 >= 0 && TableauPoids[x, y - 1] != -1)
            {
                retour = Math.Min(retour, TableauPoids[x, y - 1]);
            }
            if (y + 1 < Carte.GetInstance().Height && TableauPoids[x, y + 1] != -1)
            {
                retour = Math.Min(retour, TableauPoids[x, y + 1]);
            }

            return retour;
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
        public CellulleFranchissable(int x, int y) : base(x, y)
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

        public bool CoupeParMoi { get; internal set; }
        public bool Entamme { get; internal set; }
        private bool _SeFaitDecouper;
        public int Risque = 0;

        public bool SeFaitCouperCeTour
        {
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
            SeFaitCouperCeTour = false;
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

        public bool MiseAJourCeTour;

        //public int[,] TableauPoids = null;
        internal bool PathFindingCaisseDone = false;

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
            if (XOrigine + 1 < Width && XDestination == (XOrigine + 1) && YDestination == YOrigine)
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
                    Curiosity.nom = id.ToString();

                   //Console.Error.WriteLine("Hello Curiosity : " + id);
                }
                else if (_Opportunity == null)
                {
                    _Opportunity = new Opportunity();
                    _Opportunity.Id = id;
                    _Opportunity.nom = id.ToString();

                   //Console.Error.WriteLine("Hello Opportunity : " + id);
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
                if (Curiosity != null && _Opportunity != null)
                { 
                    Curiosity.Mate = _Opportunity;
                    _Opportunity.Mate = Curiosity;
                }
                ((PlayableBucheron)bucheron).CibleChange = false;
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
                    if (bucheron.PrecedentePosition != null && x == bucheron.PrecedentePosition.X && y == bucheron.PrecedentePosition.Y && !(x == 29 && y == 7))
                    {
                        //Console.Error.WriteLine("PAS BOUGE : " + bucheron.Id);
                        bucheron.PasBouge = true;
                    }
                    bucheron.PrecedentePosition = new CelluleInconnue(x, y);
                }
                else //ce sont les miens
                {
                    if (x == 29 && y == 7)
                    {
                        PathFindingCaisseDone = true;
                    }
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

        public void MettreAJourTableau(int entityType, int x, int y, int amount)
        {
            Cellule cell = tableau[x, y];
            switch (entityType)
            {
                case 2: //arbre
                    if (cell == null)
                    {
                        //Console.Error.WriteLine("Arbre a  x:" + x + " y:" + y);
                        cell = new Arbre(x, y);
                        MiseAJourCeTour = true;
                        //TableauPoids[x, y] = -1;
                        _Opportunity.TableauPoids[x, y] = -1;
                        Curiosity.TableauPoids[x, y] = -1;
                    }
                    if (((Arbre)cell).Energie > amount)
                    {
                        //Console.Error.WriteLine("un arbre se fait couper en x:" + x + " y:" + y);
                        ((Arbre)cell).SeFaitCouperCeTour = true;
                    }
                    else
                    {
                        ((Arbre)cell).SeFaitCouperCeTour = false;
                    }
                        ((Arbre)cell).Energie = amount;
                    break;
                case 3: //souche
                    if (cell == null)
                    {
                        MiseAJourCeTour = true;
                    }
                    cell = new Obstacle(x, y);
                    //TableauPoids[x, y] = -1;
                    _Opportunity.TableauPoids[x, y] = -1;
                    Curiosity.TableauPoids[x, y] = -1;
                    break;
                case 4: //obstacle
                    if (cell == null)
                    {
                        cell = new Obstacle(x, y);
                        MiseAJourCeTour = true;
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
                        cell = new BonusEnergie(x, y);
                        ((BonusEnergie)cell).Energie = amount;
                    }
                    ((BonusEnergie)cell).Visible = true;
                    //Console.Error.WriteLine("Je mets à jour un Bonus d'energie " + x + "," + y);
                    break;
                case 8: //Bonus Coupe
                    if (cell == null)
                    {
                        cell = new BonusCoupe(x, y);
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

        public void InitialiserTableau(int width, int height)
        {
            Width = width;
            Height = height;
            tableau = new Cellule[width, height];
            //TableauPoids = new int[width, height];

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
                tableau[0, 7] = new Camion(0, 7);
                //caisse
                tableau[29, 7] = new CaisseChampignons(29, 7);

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
            foreach (Cellule cellule in tableau)
            {
                if (cellule != null && typeof(Bonus).IsAssignableFrom(cellule.GetType()) && !(cellule.X == 29 && cellule.Y == 7))
                {
                    ((Bonus)cellule).Visible = false;
                }
            }
        }

        internal void GererSecuriteTags()
        {
            int Epsilon2 = 70;
            bool QueDuTag = true;
            bool QueDuRisque = true;
            foreach (Cellule cellule in tableau)
            {
                if (cellule != null && cellule is Arbre)
                {
                    if (!((Arbre)cellule).Tag)
                    {
                        QueDuTag = false;
                    }
                    if (((Arbre)cellule).Risque == 0)
                    {
                        QueDuRisque = false;
                    }
                    if (cellule.X == 12 && cellule.Y == 10)
                    {
                       //Console.Erroror.WriteLine("FDP : " + ((Arbre)cellule).Tag + " risque " + ((Arbre)cellule).Risque);
                    }
                }
            }
           //Console.Erroror.WriteLine("FDP V2: " + (Carte.GetInstance().NbTourMax - Carte.GetInstance().NbTour) + " QueDuRisque " + QueDuRisque + " QueDuTag :" + QueDuTag);
           //Console.Erroror.WriteLine("FDP V3 :" + ((Carte.GetInstance().NbTourMax - Carte.GetInstance().NbTour) < Epsilon2));
            if (((Carte.GetInstance().NbTourMax - Carte.GetInstance().NbTour) > Epsilon2) && (!QueDuRisque || !QueDuTag))
            {
               //Console.Erroror.WriteLine("ON COUPE UtiliserArbreTag");
                UtiliserArbreTag = false;
            }
            else
            {
               //Console.Erroror.WriteLine("UTILISER ARBRE TAG");
                UtiliserArbreTag = true;
            }



           
        }

        internal void GererRisque(AbstractBucheron bucheron)
        {
            List<Cellule> listeVoisins = new List<Cellule>();
            if (bucheron._X - 1 >= 0)
            {
                Cellule voisinGauche = new CelluleInconnue(bucheron._X - 1, bucheron._Y);
                listeVoisins.Add(voisinGauche);
            }
            if (bucheron._X + 1 < Width)
            {
                Cellule voisinDroit = new CelluleInconnue(bucheron._X + 1, bucheron._Y);
                listeVoisins.Add(voisinDroit);
            }
            if (bucheron._Y - 1 >= 0)
            {
                Cellule voisinHaut = new CelluleInconnue(bucheron._X, bucheron._Y - 1);
                listeVoisins.Add(voisinHaut);
            }
            if (bucheron._Y + 1 < Height)
            {
                Cellule voisinBas = new CelluleInconnue(bucheron._X, bucheron._Y + 1);
                listeVoisins.Add(voisinBas);
            }

            foreach (Cellule voisin in listeVoisins)
            {
                Cellule celluleTestee = tableau[voisin.X, voisin.Y];
                if (celluleTestee != null && celluleTestee is Arbre)
                {
                    if (((Arbre)celluleTestee).Tag)
                    {
                        ((Arbre)celluleTestee).Risque = 500;
                    }
                    else
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
    }

    public class Bucheron : AbstractBucheron
    {
        public Bucheron()
        {
           //Console.Error.WriteLine("Creation d'un Bucheron ennemi");
        }
    }

    public abstract class PlayableBucheron : AbstractBucheron
    {
        public PlayableBucheron Mate;
        protected List<GestionAction> ListeDesActions = new List<GestionAction>();
        public bool JeCoupe { get; set; }
        public bool JeVole { get; set; }
        public bool BesoinRenforts { get; internal set; }

        public Mode _mode, _modeDefaut;

        public Cellule Cible { get; internal set; }
        public bool JeVaisCouper1HP { get; internal set; }

        public String nom = String.Empty;

        public List<Tuple<AbstractBucheron, int>> Followers = new List<Tuple<AbstractBucheron, int>>();

        public Cellule AnciennePosition = null;

        public List<Noeud> chemin;

        public bool CibleChange = false;
        internal bool Exploration;

        public int[,] TableauPoids = null;

        public void AppelerRenforts()
        {
            BesoinRenforts = true;
        }

        public void MiseAJourCible(Cellule cellule)
        {
            if (Cible == null || !Cible.Equals(cellule))
            {
                CibleChange = true;
            }
            Cible = cellule;
        }

        public string DefinirAction()
        {
            if (Exploration)
            {
               //Console.Error.WriteLine("Exploration");
                Exploration = false;
                Cible = null;
                MiseAJourCible(null);
            }
            if (Cible != null)
            {
                MiseAJourCible(Carte.GetInstance().tableau[Cible.X, Cible.Y]);
                if (Cible != null && Cible is Obstacle )
                {
                    MiseAJourCible(null);
                    JeCoupe = false;
                    JeVaisCouper1HP = false;
                    JeVole = false;
                }
            }
            if (Cible == null)
            {
                JeCoupe = false;
                JeVole = false;
                JeVaisCouper1HP = false;
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
                        if (((Arbre)cellule).Tag)
                        {
                            ((Arbre)cellule).Risque = 500;
                        }
                        else
                        {
                            ((Arbre)cellule).Risque = 1000;
                        }
                    }
                    else
                    {
                        ListeSansRisque.Add(celluleRisque);
                    }
                }
            }

            foreach (Cellule cellule in ListeSansRisque)
            {
                Carte.GetInstance().ListePointsASurveiller.Remove(cellule);
                //Console.Error.WriteLine(nom + ": RETIRE " + cellule.X + "," + cellule.Y);
            }

            //gestion vol
            if (JeCoupe && Cible != null)
            {
                AbstractBucheron Ennemi1 = Carte.GetInstance().Ennemi1;
                AbstractBucheron Ennemi2 = Carte.GetInstance().Ennemi2;
                if ((Ennemi1.PasBouge &&
                    (Ennemi1._X == Cible.X - 1 && Ennemi1._Y == Cible.Y) ||
                    (Ennemi1._X == Cible.X + 1 && Ennemi1._Y == Cible.Y) ||
                    (Ennemi1._X == Cible.X && Ennemi1._Y == Cible.Y - 1) ||
                    (Ennemi1._X == Cible.X && Ennemi1._Y == Cible.Y + 1)
                    ) ||
                    (Ennemi2.PasBouge &&
                    (Ennemi2._X == Cible.X - 1 && Ennemi2._Y == Cible.Y) ||
                    (Ennemi2._X == Cible.X + 1 && Ennemi2._Y == Cible.Y) ||
                    (Ennemi2._X == Cible.X && Ennemi2._Y == Cible.Y - 1) ||
                    (Ennemi2._X == Cible.X && Ennemi2._Y == Cible.Y + 1)
                    )
                    )
                {
                    JeCoupe = false;
                    Cible = null;
                    MiseAJourCible(null);
                }
            }

            foreach (GestionAction gestion in ListeDesActions)
            {
                String Denomination = gestion.Denomination;
               //Console.Erroror.WriteLine(nom + ": " + gestion.GetType());
                String action = gestion.DefinirAction();
                if (action != String.Empty)
                {
                    /*if (Cible == null)
                    {
                       //Console.Erroror.WriteLine(nom + ": pas de cible");
                    }
                    else
                   //Console.Erroror.WriteLine(nom + ": cible -> " + Cible.X + "," + Cible.Y);*/
                    return action;
                }
            }
            Cible = null;
            MiseAJourCible(null);
            Exploration = true;
            //Console.Error.WriteLine(nom + ": je ne sais pas quoi faire");
            chemin = PathFinder.GridSolve(_X,_Y,29,7,TableauPoids);
            
            return "MOVE 29 7";
        }

        public void CalculerTableauUtilisateur()
        {

            for (int i = 0; i < Carte.GetInstance().Width; i++)
            {
                for (int j = 0; j < Carte.GetInstance().Height; j++)
                {
                    if (TableauPoids[i, j] != -1)
                    {
                        TableauPoids[i, j] = int.MaxValue;
                    }
                }
            }
            CalculerPoids(_X, _Y, 0);

            //Mise a jour des cellules bloquees
            for (int i = 0; i < Carte.GetInstance().Width; i++)
            {
                for (int j = 0; j < Carte.GetInstance().Height; j++)
                {
                    if (TableauPoids[i, j] == int.MaxValue && Carte.GetInstance().tableau[i, j] != null)
                    {
                        Carte.GetInstance().tableau[i, j].Bloque = true;
                    }
                }
            }
        }

        private void CalculerPoids(int i, int j, int poids)
        {


            //Console.Error.WriteLine("Calculer poids " + i + "," + j + " -- " + poids);

            if (Carte.GetInstance().tableau[i, j] != null && !Carte.GetInstance().tableau[i, j].Franchissable)
            {
                TableauPoids[i, j] = -1;
            }
            else
            {
                if (TableauPoids[i, j] <= poids)
                {
                    return;
                }
                else
                {
                    TableauPoids[i, j] = poids;
                    //Console.Error.WriteLine("J'assigne la valeur " + poids + " au tableaupoids[" + i + "," + j + "]");
                    List<Tuple<int, int>> coordonneesVoisins = new List<Tuple<int, int>>();
                    if (i - 1 >= 0)
                    {
                        coordonneesVoisins.Add(new Tuple<int, int>(i - 1, j));
                    }
                    if (j + 1 < Carte.GetInstance().Height)
                    {
                        coordonneesVoisins.Add(new Tuple<int, int>(i, j + 1));
                    }
                    if (i + 1 < Carte.GetInstance().Width)
                    {
                        coordonneesVoisins.Add(new Tuple<int, int>(i + 1, j));
                    }
                    if (j - 1 >= 0)
                    {
                        coordonneesVoisins.Add(new Tuple<int, int>(i, j - 1));
                    }

                    for (int x = 0; x < coordonneesVoisins.Count(); x++)
                    {
                        CalculerPoids(coordonneesVoisins[x].Item1, coordonneesVoisins[x].Item2, TableauPoids[i, j] + 1);
                    }
                }
            }
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
                                //Console.Error.WriteLine(nom + " Destruction du Bonus : " + cellule.X + "," + cellule.Y);
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
            nom = Id.ToString();
            TableauPoids = new int[Carte.GetInstance().Width, Carte.GetInstance().Height];

            //ajout des opérations dans le "cerveau"
            ListeDesActions.Add(new GestionActionInventaire(this)); //je vais à la caisse si j'ai moins de ??? puis je rentre au camion
            ListeDesActions.Add(new GestionActionArbre1HP(this));
            ListeDesActions.Add(new GestionActionExploration(this)); //si je n'ai que des arbres a risque ou (tag et que c'est pas le moment), je pathfind jusqu'à la caisse de champis si je l'ai pas encore fait
            ListeDesActions.Add(new GestionActionTag(this));
            ListeDesActions.Add(new GestionActionEnergie(this)); //si je vois des BOnus d'énergie à 5 ou moins j'y vais
            ListeDesActions.Add(new GestionActionBonusHache(this)); //si j'ai un bonus de Hache à moins de 5 j'y vais
            ListeDesActions.Add(new GestionActionBucheron(this)); //je vais chercher l'arbre avec le plus de poids
        }
    }

    public class Opportunity : PlayableBucheron
    {

        public Opportunity()
        {
            nom = Id.ToString();
            TableauPoids = new int[Carte.GetInstance().Width, Carte.GetInstance().Height];

            //ajout des opérations dans le "cerveau"
            ListeDesActions.Add(new GestionActionInventaire(this)); //je vais à la caisse si j'ai moins de ??? puis je rentre au camion
            ListeDesActions.Add(new GestionActionArbre1HP(this));
            ListeDesActions.Add(new GestionActionExploration(this)); //si je n'ai que des arbres a risque ou (tag et que c'est pas le moment), je pathfind jusqu'à la caisse de champis si je l'ai pas encore fait
            ListeDesActions.Add(new GestionActionTag(this));
            ListeDesActions.Add(new GestionActionEnergie(this)); //si je vois des BOnus d'énergie à 5 ou moins j'y vais
            ListeDesActions.Add(new GestionActionBonusHache(this)); //si j'ai un bonus de Hache à moins de 5 j'y vais
            ListeDesActions.Add(new GestionActionBucheron(this)); //je vais chercher l'arbre avec le plus de poids
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
            String action = PathFinding(_bucheron.Cible.X, _bucheron.Cible.Y);
            if (action.Equals(String.Empty))
            {
                action = String.Empty; //UGLY !!!
                Carte.GetInstance().tableau[_bucheron.Cible.X, _bucheron.Cible.Y].Bloque = true;
                _bucheron.MiseAJourCible(null);
            }
            return action;
        }

        protected string PathFinding_Caisse()
        {
            if (_bucheron.Cible != Carte.GetInstance().tableau[29, 7])
            {
                _bucheron.MiseAJourCible(Carte.GetInstance().tableau[29, 7]);
            }
            _bucheron.chemin = PathFinder.GridSolve(_bucheron._X, _bucheron._Y, 29, 7, _bucheron.TableauPoids);

            _bucheron.JeCoupe = false;
            _bucheron.JeVole = false;
            String action = TraiterChemin();
            return action;
        }

        /// <summary>
        /// renvoie une chaine de caractères correspondant à l'action à effectuer pour aller au prochain point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public String PathFinding(int x, int y)
        {
            if (Carte.GetInstance().MiseAJourCeTour || _bucheron.CibleChange)
            {
                bool franchissable = Carte.GetInstance().tableau[x, y].Franchissable;
                _bucheron.chemin = PathFinder.GridSolve(_bucheron._X, _bucheron._Y, x, y, _bucheron.TableauPoids);
            }
           //Console.Erroror.WriteLine("Pathfinding : " + x + "," + y);
            String action = TraiterChemin();
            return action;
        }

        public string TraiterChemin()
        {
           //Console.Erroror.WriteLine(_bucheron.nom + ": taille du chemin ->" + _bucheron.chemin.Count());
            if (_bucheron.chemin == null || _bucheron.chemin.Count() == 0)
            {
                return String.Empty;
            }
            /*if (_bucheron.chemin.Count == 1)
            {
                return "WAIT";
            }*/
           //Console.Erroror.WriteLine(_bucheron.nom + ": taille du chemin ->" + _bucheron.chemin.Count());
            int pas = Math.Min(2, _bucheron.chemin.Count() - 1);
            if (_bucheron.Energy > 0 && _bucheron.Energy <= 1)
            {
                pas = Math.Min(3, _bucheron.chemin.Count() - 1);
            }
            if (_bucheron.Energy >= 2)
            {
                pas = Math.Min(4, _bucheron.chemin.Count() -1);
            }
            if (_bucheron.chemin.Count() == 6)
            {
                pas = 3;
            }
            foreach (Noeud noeud in _bucheron.chemin)
            {
             //Console.Erroror.WriteLine(_bucheron.nom + ": Path -> " + noeud.x + "," +noeud.y + "  PAS : " + pas);
            }
            _bucheron.chemin.RemoveRange(0, pas);
            String action = "MOVE " + _bucheron.chemin[0].x + " " + _bucheron.chemin[0].y;
            //_bucheron.chemin.RemoveAt(0);
            return action;
        }

        /*public String PathFinding_Camion()
{
  //Console.Error.WriteLine("PathFinding Camion");
   return PathFinding(0, 7);
}

protected String PathFinding_CaisseChampignon()
{
   return PathFinding(29, 7);
}*/

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
            List<Tuple<Arbre, int>> ListeDesArbresPesesRapidement = new List<Tuple<Arbre, int>>();
            if (Type.Equals(type, typeof(Arbre)))
            {
                List<Tuple<Arbre, int>> ListeDesArbresPeses = new List<Tuple<Arbre, int>>();
                for (int i = 0; i < Carte.GetInstance().Width; i++) //je liste tous les arbres 
                {
                    for (int j = 0; j < Carte.GetInstance().Height; j++)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (Carte.GetInstance().UtiliserArbreTag)
                        {
                            if (cellule != null && !cellule.Bloque && cellule is Arbre && !((Arbre)cellule).SeFaitCouperCeTour && ( _bucheron.Mate.Cible != (cellule) || Carte.GetInstance().NbTour >= 10 )&& ((Arbre)cellule).Risque != 1000 && PathFinder.ObtenirPoids(cellule.X,cellule.Y,_bucheron.TableauPoids) != int.MaxValue)
                            {
                                //Console.Error.WriteLine(_bucheron.nom + ": " + cellule.X + "," + cellule.Y + " -> " + CalculPoidsArbre((Arbre)cellule,true));
                                ListeDesArbresPeses.Add(new Tuple<Arbre, int>((Arbre)cellule, CalculPoidsArbre((Arbre)cellule, true)));
                            }
                        }
                        else
                        {
                            if (cellule != null && !cellule.Bloque && cellule is Arbre && !((Arbre)cellule).SeFaitCouperCeTour && ((Arbre)cellule).Energie < 6 && _bucheron.Mate.Cible != (cellule) && !((Arbre)cellule).Tag && ((Arbre)cellule).Risque != 1000 && PathFinder.ObtenirPoids(cellule.X, cellule.Y, _bucheron.TableauPoids) != int.MaxValue)
                            {
                                ListeDesArbresPeses.Add(new Tuple<Arbre, int>((Arbre)cellule, CalculPoidsArbre((Arbre)cellule, true)));
                            }
                        }
                    }
                }

                /*foreach(Tuple<Arbre,int> tuple in ListeDesArbresPeses)
                {
                   //Console.Error.WriteLine(_bucheron.nom + ": " + tuple.Item1.X + "," + tuple.Item1.Y + " --> " + tuple.Item2);
                }*/

                Tuple<Arbre, int> arbre = null;
                if (ListeDesArbresPeses.Count > 0)
                {
                    int PoidsMin = ListeDesArbresPeses.Select(p => p.Item2).Min();
                    List<Tuple<Arbre, int>> ListeDesMeilleurs = ListeDesArbresPeses.Where(p => p.Item2 == PoidsMin).ToList();
                    int VieMin = ListeDesMeilleurs.Select(p => ((Arbre)p.Item1).Energie).Min();
                    List<Tuple<Arbre, int>> ListeDesMoinsDeVie = ListeDesMeilleurs.Where(p => ((Arbre)p.Item1).Energie == VieMin).ToList();
                    arbre = ListeDesMoinsDeVie.FirstOrDefault();
                }


                if (arbre == null)
                {
                    //Console.Error.WriteLine(_bucheron.nom + ": je n'ai pas trouvé d'arbre");
                    _bucheron.MiseAJourCible(null);
                }
                else
                {
                    Cellule cellule = Carte.GetInstance().tableau[arbre.Item1.X, arbre.Item1.Y];
                    _bucheron.MiseAJourCible(cellule);
                }
            }
            else
            {
                throw new Exception("Type non Connu : " + type);
            }
        }

        private int CalculPoidsArbre(Arbre cellule, bool calculRapide)
        {
            int BonusHache = 0;
            if (_bucheron.BonusHache > 0)
            {
                BonusHache = 1;
            }
            int TempsCoupe = (int)Math.Ceiling((double)cellule.Energie / (1 + (double)BonusHache));

            int poids = cellule.Risque + PathFinder.ObtenirPoids(cellule.X, cellule.Y,_bucheron.TableauPoids) + TempsCoupe;
            return poids;
        }

        public bool IlExistePasLoin(Type type, int distance = 5)
        {
            int Epsilon = 50;
            bool existe = false;
            for (int i = _bucheron._X - distance; i <= _bucheron._X + distance; i++)
            {
                for (int j = _bucheron._Y - distance; j <= _bucheron._Y + distance; j++)
                {
                    if (i >= 0 && i < Carte.GetInstance().Width && j >= 0 && j < Carte.GetInstance().Height)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (cellule != null && !cellule.Bloque && PathFinder.ObtenirPoids(cellule.X, cellule.Y, _bucheron.TableauPoids) <= distance)
                        {
                            if (Type.Equals(type,typeof(BonusEnergie)))
                            {
                                if (type.IsAssignableFrom(cellule.GetType()) && !(cellule is CaisseChampignons))
                                {
                                   //Console.Erroror.WriteLine("gotcha 1: " + cellule.X + "," + cellule.Y);
                                    existe = true;
                                }
                                else if (cellule is BonusEnergie && Carte.GetInstance().NbTourMax - Carte.GetInstance().NbTour <= Epsilon)
                                {
                                   //Console.Erroror.WriteLine("gotcha 2: " + cellule.X + "," + cellule.Y);
                                    existe = true;
                                }
                            }
                            else
                            {
                                if (type.IsAssignableFrom(cellule.GetType()) && _bucheron.TableauPoids[cellule.X, cellule.Y] <= distance)
                                {
                                    //Console.Error.WriteLine(_bucheron.nom + ": " + cellule.X + "," + cellule.Y +" poids ->" + PathFinder.ObtenirPoids(cellule.X, cellule.Y, _bucheron.TableauPoids));
                                    existe = true;
                                }
                            }
                        }
                    }
                }
            }
            return existe;
        }

        public void TrouverEnVisu(Type type, int distance = 5)
        {
            int Epsilon = 70;
            List<Tuple<Cellule, float>> listeObjets = new List<Tuple<Cellule, float>>();
            for (int i = _bucheron._X - distance; i <= _bucheron._X + distance; i++)
            {
                for (int j = _bucheron._Y - distance; j <= _bucheron._Y + distance; j++)
                {
                    if (i >= 0 && i < Carte.GetInstance().Width && j >= 0 && j < Carte.GetInstance().Height)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        if (cellule != null && !cellule.Bloque && PathFinder.ObtenirPoids(cellule.X, cellule.Y, _bucheron.TableauPoids) <= distance)
                        {
                            if (type.IsAssignableFrom(cellule.GetType()))
                            {
                                if (Type.Equals(type, typeof(BonusEnergie)))
                                {
                                    if (!(cellule is CaisseChampignons))
                                    {
                                        listeObjets.Add(new Tuple<Cellule, float>(cellule, ((BonusEnergie)cellule).Energie));
                                    }
                                    else if (Carte.GetInstance().NbTourMax - Carte.GetInstance().NbTour <= Epsilon)
                                    {
                                        listeObjets.Add(new Tuple<Cellule, float>(cellule, ((BonusEnergie)cellule).Energie));
                                    }
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
            if (_bucheron.Cible != null && type.IsAssignableFrom(_bucheron.Cible.GetType()) && !_bucheron.Cible.Equals(pire)) //Evite la danse ?
            {
                return;
            }
            if (meilleur != null)
            {
                _bucheron.MiseAJourCible(meilleur.Item1);
            }
            else
            {
                _bucheron.MiseAJourCible(null);
            }
        }
    }

    public class GestionActionArbre1HP : GestionAction
    {
        public GestionActionArbre1HP(PlayableBucheron bucheron) : base(bucheron)
        {

        }
        public override string DefinirAction()
        {
            String action = String.Empty;
            if (!_bucheron.JeCoupe && !_bucheron.JeVole && _bucheron.inventory <= 0)
            {
                if (_bucheron.Cible != null && _bucheron.JeVaisCouper1HP)
                {
                    if (Carte.GetInstance().EstACote(_bucheron._X, _bucheron._Y, _bucheron.Cible.X, _bucheron.Cible.Y))
                    {
                        action = "CUT " + _bucheron.Cible.X + " " + _bucheron.Cible.Y;
                        _bucheron.JeVaisCouper1HP = false;
                    }
                    else
                    {
                        action = PathFinding_Cible();
                    }
                }
                else
                {
                    List<Arbre> listeDesArbres1HP = new List<Arbre>();
                    foreach (Cellule cellule in Carte.GetInstance().tableau)
                    {
                        if (cellule != null && cellule is Arbre)
                        {
                            Arbre arbre = cellule as Arbre;
                            if (arbre.Risque == 0 && !arbre.Tag && !arbre.Bloque && !arbre.Entamme && arbre.Energie == 1 && _bucheron.Mate.Cible != (cellule) && PathFinder.ObtenirPoids(arbre.X,arbre.Y,_bucheron.TableauPoids) <= 4)
                            {
                                listeDesArbres1HP.Add(arbre);
                            }
                        }
                    }
                    if (listeDesArbres1HP.Count() > 0)
                    {
                        Arbre arbre = listeDesArbres1HP.First();
                        _bucheron.MiseAJourCible(arbre);
                        if (Carte.GetInstance().EstACote(_bucheron._X, _bucheron._Y, _bucheron.Cible.X, _bucheron.Cible.Y))
                        {
                            action = "CUT " + _bucheron.Cible.X + " " + _bucheron.Cible.Y;
                            _bucheron.JeVaisCouper1HP = false;
                        }
                        else
                        {
                            action = PathFinding_Cible();
                            _bucheron.JeVaisCouper1HP = true;
                        }
                    }
                
                }
            }
            return action;
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
            String action = String.Empty;
            if(_bucheron.Cible != null && _bucheron.Cible is Arbre && Carte.GetInstance().EstACote(_bucheron._X, _bucheron._Y, _bucheron.Cible.X, _bucheron.Cible.Y))
            {
                return action;
            }
            if (!_bucheron.JeCoupe && !_bucheron.JeVole && IlExistePasLoin(typeof(BonusEnergie)) && _bucheron.Energy < 90)
            {
                TrouverEnVisu(typeof(BonusEnergie));
                if (_bucheron.Cible == null)
                {
                    throw new Exception("je n'ai pas trouvé le BonusEnergie");
                }
               //Console.Erroror.WriteLine("cible : " + _bucheron.Cible.X + "," + _bucheron.Cible.Y);
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
            int Epsilon = 60;
            int Epsilon2 = 30;
            if (_bucheron.inventory > 0)
            {
                _bucheron.JeCoupe = false;
                _bucheron.JeVole = false;

                if (_bucheron.Cible == null && IlExistePasLoin(typeof(BonusEnergie),8))
                {
                    TrouverEnVisu(typeof(BonusEnergie),8);
                    action = PathFinding_Cible();
                    return action;
                }


                if (_bucheron.Energy >= Epsilon)
                {
                    action = PathFinding_Camion();
                   //Console.Error.WriteLine(_bucheron.nom + ": PathFinding CAMION " + action);
                }
                else if (_bucheron._X < 15)
                {
                    action = PathFinding_Camion();
                   //Console.Error.WriteLine(_bucheron.nom + ": PathFinding CAMION " + action);
                }
                else if (Carte.GetInstance().NbTourMax - Carte.GetInstance().NbTour >= Epsilon2)
                {
                   //Console.Error.WriteLine(_bucheron.nom + ": PathFinding CAISSE " + action);
                    action = PathFinding_Caisse();
                }
                else
                {
                    action = PathFinding_Camion();
                   //Console.Error.WriteLine(_bucheron.nom + ": PathFinding CAMION " + action);
                }
            }
            else
            {

                if (_bucheron.Cible != null && _bucheron.Cible is Camion && _bucheron._X == 0 && (_bucheron._Y == 6 || _bucheron._Y == 7 || _bucheron._Y == 8))
                {
                    _bucheron.Cible = null;
                    _bucheron.MiseAJourCible(null);
                }
            }

            return action;
        }

        private string PathFinding_Camion()
        {
            if (_bucheron.Cible != Carte.GetInstance().tableau[0, 7])
            {
                _bucheron.MiseAJourCible(Carte.GetInstance().tableau[0, 7]);
            }
            _bucheron.JeCoupe = false;
            _bucheron.JeVole = false;
           //Console.Error.WriteLine("cible PathFinding : " + _bucheron.Cible.X + "," + _bucheron.Cible.Y);
            _bucheron.chemin = PathFinder.GridSolve(_bucheron._X, _bucheron._Y, _bucheron.Cible.X, _bucheron.Cible.Y, _bucheron.TableauPoids);
           //Console.Error.WriteLine("CHEMIN COUNT : " + _bucheron.chemin.Count());
            String action = TraiterChemin();
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
               //Console.Erroror.WriteLine("je coupe");
                //Console.Error.WriteLine("Je coupe")
                if (Carte.GetInstance().tableau[_bucheron.Cible.X, _bucheron.Cible.Y] == null)
                {
                    throw new Exception("FUCK !");
                }
                _bucheron.JeCoupe = true;
                action = "CUT " + _bucheron.Cible.X + " " + _bucheron.Cible.Y;
                ((Arbre)_bucheron.Cible).SeFaitCouperCeTour = true;
                return action;
            }
            if (_bucheron.Cible == null)
            {
                JeTrouveLePlusRentable(typeof(Arbre));
               //Console.Erroror.WriteLine("c'est le plus rentable");
               //Console.Erroror.WriteLine("ma cible est : " + _bucheron.Cible.X + "," + _bucheron.Cible.Y);
            }

            if (_bucheron.Cible != null && _bucheron.Cible is Arbre)
            {
               //Console.Erroror.WriteLine("la cible est un arbre");
                if (Carte.GetInstance().EstACote(_bucheron._X, _bucheron._Y, _bucheron.Cible.X, _bucheron.Cible.Y))
                {
                    _bucheron.JeCoupe = true;
                    action = "CUT " + _bucheron.Cible.X + " " + _bucheron.Cible.Y;
                    ((Arbre)_bucheron.Cible).SeFaitCouperCeTour = true;
                }
                else
                {
                    action = PathFinding_Cible();
                }
            }
           //Console.Erroror.WriteLine(_bucheron.nom + ": " + action);
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
                //Console.Error.WriteLine(_bucheron.nom + ": on marche sur mes pas");
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

                //Console.Error.WriteLine(_bucheron.nom + ": on marche sur mes pas");
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
                //Console.Error.WriteLine(_bucheron.nom + ": je suis suivi");
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
            if (_bucheron.JeCoupe)
            {
                return action;
            }
            if (Carte.GetInstance().TagLeft > 0)
            {
               //Console.Error.WriteLine("il me reste " + Carte.GetInstance().TagLeft + " ");
                 if (_bucheron.Cible == null)
                 {
                   //Console.Erroror.WriteLine(_bucheron.nom + ": Je trouve plus rentable pour tag");
                    JeTrouveArbreLePlusRentablePourTag();
                }
                if (_bucheron.Cible != null && _bucheron.Cible is Arbre)
                {
                  //Console.Erroror.WriteLine(_bucheron.nom + " " + _bucheron.GetType() + ": type de cible ->" + _bucheron.Cible.GetType());
                  //Console.Erroror.WriteLine(_bucheron.nom + ": cible -> " + _bucheron.Cible.X + "," + _bucheron.Cible.Y);
                    if (((Arbre)_bucheron.Cible).Tag)
                    {
                        return action;
                    }
                    if (Carte.GetInstance().EstACote(_bucheron._X, _bucheron._Y, _bucheron.Cible.X, _bucheron.Cible.Y))
                    {
                       //Console.Error.WriteLine(_bucheron.nom + ": " + _bucheron._X + "," + _bucheron._Y + " est a cote de " + _bucheron.Cible.X + "," + _bucheron.Cible.Y);
                        ((Arbre)_bucheron.Cible).Tag = true;
                        if (((Arbre)_bucheron.Cible).Risque == 1000)
                        {
                            ((Arbre)_bucheron.Cible).Risque = 500;
                        }
                        action = "TAG " + _bucheron.Cible.X + " " + _bucheron.Cible.Y;
                        _bucheron.Cible = null;
                    }
                    else
                    {
                        action = PathFinding_Cible();
                    }
                    Carte.GetInstance().TagLeft--;
                }
            }
            return action;
        }

        private void JeTrouveArbreLePlusRentablePourTag()
        {
            List<Tuple<Cellule, float>> ListeTuple = new List<Tuple<Cellule, float>>();
            foreach (Cellule cellule in Carte.GetInstance().tableau)
            {
                if (cellule != null && cellule is Arbre && !cellule.Bloque && !((Arbre)cellule).Tag && !((Arbre)cellule).Entamme && ((Arbre)cellule).Energie != 1 && ((Arbre)cellule).Energie != 6 && _bucheron.Mate.Cible != (cellule) && PathFinder.ObtenirPoids(cellule.X,cellule.Y,_bucheron.TableauPoids) != int.MaxValue)
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
                    _bucheron.MiseAJourCible(piege.Item1);
                }
            }
        }

        private float CalculPerturbationArbre(Arbre cellule)
        {
            float poids = 0;

            poids = cellule.Risque + Convert.ToSingle(2 * (Math.Floor((double)(Math.Abs(cellule.X - 0) + Math.Abs(cellule.Y - 7)) / (double)4)) + cellule.Energie);

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

            if (_bucheron.BonusHache <= 1 && !_bucheron.JeCoupe && !_bucheron.JeVole && IlExistePasLoin(typeof(BonusCoupe)))
            {
                TrouverEnVisu(typeof(BonusCoupe));
                if (_bucheron.Cible == null)
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
                    _bucheron.MiseAJourCible(null);
                }
                else
                {
                    if (!(_bucheron.Cible is Arbre) || !((Arbre)_bucheron.Cible).SeFaitCouperCeTour)
                    {
                        //Console.Error.WriteLine("ce FDP m'a grille");
                        _bucheron.JeVole = false;
                        _bucheron.MiseAJourCible(null);
                    }
                    else
                    {
                        action = "WAIT";
                        return action;
                    }
                }
            }
            if (!_bucheron.JeCoupe && JePeuxVolerArbre())
            {
                if (Carte.GetInstance().EstACote(_bucheron._X, _bucheron._Y, _bucheron.Cible.X, _bucheron.Cible.Y))
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
            Cellule cible = _bucheron.Cible;
            bool jePeuxVoler = false;
            foreach (Cellule cellule in Carte.GetInstance().tableau)
            {
                if (cellule != null && cellule is Arbre && !((Arbre)cellule).Bloque && ((Arbre)cellule).SeFaitCouperCeTour && !((Arbre)cellule).Tag && PathFinder.ObtenirPoids(cellule.X, cellule.Y, _bucheron.TableauPoids) != int.MaxValue)
                {
                    //je regarde si j'aurai le temps d'y aller
                    int distance = PathFinder.ObtenirPoids(cellule.X, cellule.Y,_bucheron.TableauPoids);//PathFinder.AStarSolve(_bucheron._X, _bucheron._Y, cellule.X, cellule.Y, false).Count();
                    if (Math.Floor((double)distance / (double)4) <= ((Arbre)cellule).Energie && _bucheron.Energy >= 2 * (int)distance / 4 + Math.Max(distance % 4 - 2, 0))
                    {
                        _bucheron.MiseAJourCible(cellule);
                        if (IlYAUnEnnemiACoteDeMaCible())
                        {
                            return true;
                        }
                    }
                }
            }
            _bucheron.MiseAJourCible(cible);
            return jePeuxVoler;
        }

        private bool IlYAUnEnnemiACoteDeMaCible()
        {
            bool present = false;

            AbstractBucheron Ennemi1 = Carte.GetInstance().Ennemi1;
            AbstractBucheron Ennemi2 = Carte.GetInstance().Ennemi2;

            if (Ennemi1._X == _bucheron.Cible.X - 1 && Ennemi1._Y == _bucheron.Cible.Y)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.Cible.X - 1 && Ennemi2._Y == _bucheron.Cible.Y)
            {
                present = true;
            }
            if (Ennemi1._X == _bucheron.Cible.X + 1 && Ennemi1._Y == _bucheron.Cible.Y)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.Cible.X + 1 && Ennemi2._Y == _bucheron.Cible.Y)
            {
                present = true;
            }
            if (Ennemi1._X == _bucheron.Cible.X && Ennemi1._Y == _bucheron.Cible.Y - 1)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.Cible.X && Ennemi2._Y == _bucheron.Cible.Y - 1)
            {
                present = true;
            }
            if (Ennemi1._X == _bucheron.Cible.X && Ennemi1._Y == _bucheron.Cible.Y + 1)
            {
                present = true;
            }
            if (Ennemi2._X == _bucheron.Cible.X && Ennemi2._Y == _bucheron.Cible.Y + 1)
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
            int Epsilon = 70;
          //Console.Error.WriteLine(_bucheron.nom + ": coucou");
            String action = String.Empty;
            bool bloque = true;
            foreach(Cellule cellule in Carte.GetInstance().tableau)
            {
                if (cellule != null && !cellule.Bloque && cellule is Arbre && !((Arbre)cellule).SeFaitCouperCeTour && ( Carte.GetInstance().UtiliserArbreTag || ((Arbre)cellule).Energie < 6) && ( _bucheron.Mate.Cible != (cellule) || Carte.GetInstance().NbTour >= 10 ) && ( !((Arbre)cellule).Tag || Carte.GetInstance().UtiliserArbreTag) && ((Arbre)cellule).Risque != 1000 && PathFinder.ObtenirPoids(cellule.X, cellule.Y, _bucheron.TableauPoids) != int.MaxValue)
                {

                   //Console.Error.WriteLine(_bucheron.nom + ": PAS BLOQUE " + cellule.X + "," + cellule.Y);
                    bloque = false;
                }
            }
            if (bloque) //&& ! Carte.GetInstance().PathFindingCaisseDone)
            {
                action = PathFinding_Caisse();
                _bucheron.Exploration = true;
               //Console.Error.WriteLine(_bucheron.nom + ": je pars en vadrouille");
            }
            return action;
        }
    }


    public class Player
    {
        static void Main(string[] args)
        {
           //Console.Error.WriteLine("lancement");
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
            //Console.Error.WriteLine("N:" + N);
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
                DateTime dt1 = DateTime.Now;
                Carte.GetInstance().NbTour++;
                Carte.GetInstance().GererSecuriteTags();
                Carte.GetInstance().MettreVisibiliteBonusFalse();
                Carte.GetInstance().MiseAJourCeTour = false;
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
                //if (carte.MiseAJourCeTour || carte.NbTour == 1)
                //{
                   //Console.Error.WriteLine("debut calcul 1");
                    carte.Curiosity.CalculerTableauUtilisateur();
                   //Console.Error.WriteLine("fin calcul 1");
                    carte._Opportunity.CalculerTableauUtilisateur();
                //}
                carte.Curiosity.VerifierBonus();
                carte._Opportunity.VerifierBonus();
                String CuriosityAction = carte.Curiosity.DefinirAction();
                String OpportunityAction = carte._Opportunity.DefinirAction();

                if (((PlayableBucheron)carte.Curiosity).BesoinRenforts || ((PlayableBucheron)carte._Opportunity).BesoinRenforts)
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
                DateTime dt2 = DateTime.Now;
                TimeSpan span = dt2 - dt1;
                int ms = (int)span.TotalMilliseconds;
               //Console.Error.WriteLine("TEMPS DE CALCUL : " + ms);
                Console.Out.WriteLine(CuriosityAction);
                Console.Out.WriteLine(OpportunityAction);
            }
        }
    }
}