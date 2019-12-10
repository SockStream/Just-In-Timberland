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

        public PathFinder()
        {
        }
        public List<Noeud> Solve(int XOrigine, int YOrigine, int XDestination, int YDestination, bool Franchissable)
        {
            //Console.Error.WriteLine("PathFinding de " + XOrigine + "," + YOrigine + " vers " + XDestination + "," + YDestination);
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

        private IEnumerable<Noeud> ObtenirVoisins(Noeud noeud)
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
        public bool SeFaitDecouper { get; internal set; }
        public Cellule(int x, int y)
        {
            X = x;
            Y = y;
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

    public class BonusEnergie : CellulleFranchissable
    {
        public int Energie { get; internal set; }

        public BonusEnergie(int x, int y) : base(x, y)
        {

        }
    }

    public class BonusCoupe : CellulleFranchissable
    {
        public int NbTours { get; internal set; }

        public BonusCoupe(int x, int y) : base(x, y)
        {

        }
    }

    public class CaisseChampignons : CellulleFranchissable
    {
        public CaisseChampignons(int x, int y) : base(x, y)
        {

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
        public int NbTags { get;  set; }

        public AbstractBucheron Curiosity = null;
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
            //Console.Error.WriteLine("mise a jour Bucheron : " + id + "," + playerId);
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
                bucheron.BesoinRenforts = false;
                bucheron._X = x;
                bucheron._Y = y;
                bucheron.Energy = energy;
                bucheron.BonusHache = axeBonus;
                bucheron.inventory = inventory;
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
                        //Console.Error.WriteLine("Arbre a  x:" + x + " y:" + y);
                        cell = new Arbre(x,y);
                    }
                    if (((Arbre)cell).Energie > amount)
                    {
                        //Console.Error.WriteLine("un arbre se fait couper en x:" + x + " y:" + y);
                        cell.SeFaitDecouper = true;
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
                    break;
                case 7: //Bonus Energie
                    if (cell == null)
                    {
                        cell = new BonusEnergie(x,y);
                        ((BonusEnergie)cell).Energie = amount;
                    }
                    break;
                case 8: //Bonus Coupe
                    if (cell == null)
                    {
                        cell = new BonusCoupe(x,y);
                        ((BonusCoupe)cell).NbTours = amount;
                    }
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
            NbTags = 0;
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
        public bool JeCoupe { get; private set; }
        public bool BesoinRenforts { get; internal set; }

        public Mode _mode, _modeDefaut;

        protected int[] cible = null;

        protected String nom = String.Empty;

        public void AppelerRenforts()
        {
            BesoinRenforts = true;
        }

        public string DefinirAction()
        {
            String action = null;

            if (_X == 0 && _Y == 7 && inventory <= 0)
            {
                //Console.Error.WriteLine(nom + ": j'ai ramené mon chargement");
            }
            //mode bucheron "optimisé"

            if (inventory > 0)//si on a qqch dans l'inventaire
            {
                //Console.Error.WriteLine(nom + ": J'ai du bois, je retourne au camion");
                action = PathFinding_Camion();//path_finding_camion
                JeCoupe = false;
                return action;
            }

            if (JeCoupe)
            {
                //Console.Error.WriteLine(nom + ": Je suis en train de couper");
                Cellule cellule = Carte.GetInstance().tableau[cible[0], cible[1]];
                if (cellule is Obstacle) //je me suis fait voler l'arbre
                {
                    JeCoupe = false;
                    //Console.Error.WriteLine(nom + ": je crois que je me suis fait voler");
                }
                else
                {
                    ((Arbre)cellule).CoupeParMoi = true;
                    ((Arbre)cellule).SeFaitDecouper = true;
                    //Console.Error.WriteLine(nom + ": Je coupe");
                    action = "CUT " + cible[0] + " " + cible[1];
                    return action;
                }
            }
            /*if (JePeuxVolerArbre() && !JeCoupe) //intension != couper permet de ne pas laisser un arbre en cours de coupe pour aller voler
            {
                Console.Error.WriteLine(nom + ": Je vois que je peux voler un arbre -> " + cible[0] + "," + cible[1]);
                if (Carte.GetInstance().EstACote(_X, _Y, cible[0], cible[1]))
                {
                    JeCoupe = false;
                    action = "WAIT";
                    return action;
                }
                else
                {
                    Console.Error.WriteLine("Pathfind");
                    action = PathFinding(cible[0], cible[1], true);
                    JeCoupe = false;
                    return action;
                }
            }
            else if (JeVoisUnBonusDeHache() && BonusHache <= 2)
            {
                Console.Error.WriteLine(nom + ": Je vois un Bonus de Hache ->" + cible[0] + "," + cible[1]);
                action = PathFinding(cible[0], cible[1], false);
                JeCoupe = false;
                return action;
            }*/
            //else
            //{
                JeChercheArbreLePlusRentable(); // ne pas prendre en compte les arbres tagges
                if (cible != null) //au cas ou je n'aurai pas trouvé d'arbre
                {
                    //Console.Error.WriteLine(nom + ": Je vais couper l'arbre ->" + cible[0] + "," + cible[1]);
                    /*if ((Carte.GetInstance().Ennemi1.PeutVoler(cible) || Carte.GetInstance().Ennemi2.PeutVoler(cible)) && !((Arbre)Carte.GetInstance().tableau[cible[0], cible[1]]).Securise)
                    {
                        if (CibleEstACote())
                        {
                            Console.Error.WriteLine(nom + ": par sécurité je vais Attendre et demander des renforts");
                            action = "WAIT";
                            AppelerRenforts();
                            JeCoupe = false;
                        }
                        else
                        {
                            Console.Error.WriteLine(nom + ": Je vais me rapprocher de l'arbre avec un ennemi a proximité ->" + cible[0] + "," + cible[1]);
                            action = PathFinding(cible[0], cible[1]);
                            JeCoupe = false;
                        }
                    }
                    else
                    {*/
                        if (Carte.GetInstance().EstACote(_X, _Y, cible[0], cible[1]))
                        {
                            //Console.Error.WriteLine(nom + ": Je coupe l'arbre");
                            ((Arbre)Carte.GetInstance().tableau[cible[0], cible[1]]).SeFaitDecouper = true;
                            action = "CUT " + cible[0] + " " + cible[1];
                            JeCoupe = true;
                        }
                        else
                        {
                            //Console.Error.WriteLine(nom + ": PathFinding");
                            action = PathFinding(cible[0], cible[1]);
                            JeCoupe = false;
                        }
                    //}
                    return action;
                }
            //}
            //Console.Error.WriteLine(nom + " Je n'ai rien d'autre a faire, je vais Explorer");
            action = PathFinding(29, 7); //appeler la méthode du mode exploration
            JeCoupe = false;
            return action;
        }

        private bool PeutVoler(int[] cible)
        {
            int Distance = Math.Abs(cible[0] - _X) + Math.Abs(cible[1] - _Y);
            if (Distance <= 4)
            {
                return true;
            }
            return false;
        }

        private void JeChercheArbreLePlusRentable()
        {
            List<int[]> ListeDesPositions = new List<int[]>();
            for (int i = 0; i < Carte.GetInstance().Width; i++)
            {
                for (int j = 0; j < Carte.GetInstance().Height; j++)
                {
                    Cellule cellule = Carte.GetInstance().tableau[i, j];
                    if (cellule != null && cellule is Arbre && !((Arbre)cellule).SeFaitDecouper && !((Arbre)cellule).Tag)
                    {
                        ListeDesPositions.Add(new int[] { i, j, 0 });
                    }
                }
            }
            int[] arbre = null;
            if (ListeDesPositions.Count > 0)
            {
                foreach (int[] position in ListeDesPositions)
                {
                    position[2] = (Math.Abs(_X - position[0]) + Math.Abs(_Y - position[1])/2)
                        + ((Arbre)Carte.GetInstance().tableau[position[0], position[1]]).Energie +
                        (Math.Abs(0 - position[0]) + Math.Abs(7 - position[1])/2);
                    /*position[2] = NombreDeToursPourAllerEn(position[0], position[1]) + //nb de cases dans le pathfind divisé par 2 ou 4
                        ((Arbre)Carte.GetInstance().tableau[position[0], position[1]]).Energie +
                        NombreDeToursPourRetournerAuCamionDepuis(position[0], position[1]); //nb de cases dans le pathfind divisé par 2 ou 4
                        */
                }
                int NbToursMin = ListeDesPositions.Select(p => p[2]).Min();
                arbre = ListeDesPositions.Where(p => p[2] == NbToursMin).FirstOrDefault();
            }


            if (arbre == null)
            {
                //Console.Error.WriteLine(nom + ": je n'ai pas trouvé d'arbre");
                cible = null;
            }
            else
            {
                cible = arbre;
            }
        }

        private int NombreDeToursPourRetournerAuCamionDepuis(int X, int Y)
        {
            return NombreDeToursPourAllerDepuisEn(X, Y, 0, 7);
        }

        private int NombreDeToursPourAllerEn(int x, int y)
        {
            return NombreDeToursPourAllerDepuisEn(_X, _Y, x, y);
        }

        public int NombreDeToursPourAllerDepuisEn(int XOrigine, int YOrigine, int XDestination, int YDestination, bool speed = false)
        {
            PathFinder pathFinder = new PathFinder();
            bool franchissable = Carte.GetInstance().tableau[XDestination, YDestination].Franchissable;
            int NombreDeToursBrut = pathFinder.Solve(XOrigine, YOrigine, XDestination, YDestination, franchissable).Count();
            if (speed)
            {
                return NombreDeToursBrut / 4;
            }
            else
            {
                return NombreDeToursBrut / 2;
            }
        }

        private bool JeVoisUnBonusDeHache()
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
                            cible = new int[] { i, j };
                            return true;
                        }
                    }
                }
            }
            return BonusExiste;
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
            for (int i = _X - 5; i < _X + 5; i++)
            {
                for (int j = _Y - 5; j < _Y + 5; j++)
                {
                    if (i >= 0 && i < Carte.GetInstance().Width && j >= 0 && j < Carte.GetInstance().Height)
                    {
                        Cellule cellule = Carte.GetInstance().tableau[i, j];
                        AbstractBucheron Ennemi1 = Carte.GetInstance().Ennemi1;
                        AbstractBucheron Ennemi2 = Carte.GetInstance().Ennemi2;
                        if (cellule is Arbre && (Carte.GetInstance().EstACote(Ennemi1._X, Ennemi1._Y,i, j) || Carte.GetInstance().EstACote(Ennemi2._X,Ennemi2._Y,i, j)) && ((Arbre)cellule).Energie <= 4 && ((Arbre)cellule).SeFaitDecouper && !((Arbre)cellule).EstSquatte)
                        {
                            ((Arbre)cellule).EstSquatte = true;
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
            return Carte.GetInstance().EstACote(_X, _Y, cible[0], cible[1]);
        }

       


        protected String PathFinding_Camion()
        {
            //Console.Error.WriteLine("PathFinding Camion");
            return PathFinding(0, 7);
        }

        protected String PathFinding_CaisseChampignon()
        {
            return PathFinding(29, 7);
        }

        /// <summary>
        /// renvoie une chaine de caractères correspondant à l'action à effectuer pour aller au prochain point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        protected String PathFinding(int x, int y, bool speed = false)
        {
            PathFinder pathFinder = new PathFinder();

            bool franchissable = Carte.GetInstance().tableau[x, y].Franchissable;
            List<Noeud> Chemin = pathFinder.Solve(_X, _Y, x, y,franchissable);
            if (Chemin.Count == 0)
            {
                throw new Exception("Chemin Impossible");
            }
            int pas;
            if (speed)
            {
                pas = Math.Min(4, Chemin.Count()) - 1;
            }
            else
            {
                pas = Math.Min(2, Chemin.Count()) - 1;
            }
            return "MOVE " + Chemin[pas].x + " " + Chemin[pas].y;
        }
    }

    public class Bucheron : AbstractBucheron
    {
        public Bucheron()
        {
            //Console.Error.WriteLine("Creation d'un Bucheron ennemi");
        }
    }

    public class Curiosity : AbstractBucheron
    {
        public Curiosity()
        {
            //Console.Error.WriteLine("Hello Curiosity");
            _mode = Mode.MODE_OPPORTUNISTE;
            _modeDefaut = Mode.MODE_OPPORTUNISTE;
            nom = "Curiosity";
        }
    }

    public class Opportunity : AbstractBucheron
    {

        public Opportunity()
        {
            //Console.Error.WriteLine("Hello Opportunity");
            _mode = Mode.MODE_OPPORTUNISTE;
            _modeDefaut = Mode.MODE_OPPORTUNISTE;
            nom = "Opportunity";
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
            Console.Error.WriteLine(ligne);
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
            Console.Error.WriteLine( N);
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
                Console.Error.WriteLine(ligne);

                inputs = ligne.Split(' ');
                int cellType = int.Parse(inputs[0]); // 0 for truck, 1 for energy cells
                int x = int.Parse(inputs[1]); // position of the entity
                int y = int.Parse(inputs[2]); // position of the entity
            }

            //
            Carte carte = Carte.GetInstance();
            carte.InitialiserTableau(width, height);
            carte.NbTags = N;

            // game loop
            while (true)
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
                Console.Error.WriteLine("score ==>" +ligne);
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
                    Console.Error.WriteLine(ligne);
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
                Console.Error.WriteLine(ligne);
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
                    Console.Error.WriteLine(ligne);
                    inputs = ligne.Split(' ');
                    int id = int.Parse(inputs[0]);
                    int entityType = int.Parse(inputs[1]); // 2 for tree, 3 for stump, 4 for fence, 7 for energy bonus, 8 for axe bonus
                    int x = int.Parse(inputs[2]);
                    int y = int.Parse(inputs[3]);
                    int amount = int.Parse(inputs[4]); // depends of type (see rules)
                    carte.MettreAJourTableau(entityType, x, y, amount);
                }
                String CuriosityAction = carte.Curiosity.DefinirAction();
                String OpportunityAction =carte._Opportunity.DefinirAction();

                if (carte.Curiosity.BesoinRenforts || carte._Opportunity.BesoinRenforts)
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
                Console.Out.WriteLine(CuriosityAction);
                Console.Out.WriteLine(OpportunityAction);
            }
        }
    }
}