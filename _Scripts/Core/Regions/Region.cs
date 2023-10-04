using System.Collections.Generic;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.ResorcesAndRecipes;

namespace _Scripts.Core.Regions
{
    public class Region
    {
        public uint OwnerID { get; private set; }
        public List<Region> Adjacents { get; set; }
        public List<EconomyEntity> EconomyEntities { get; set; } = new();
        public List<Pop> Pops { get; set; } = new();
        public string Name { get; set; }
        public int ID;

        public Region(int id, uint ownerID = uint.MaxValue)
        {
            ID = id;
            OwnerID = ownerID;
            Name = $"Region {id}";
        }

        public EconomyEntity AddEconomyEntity(int recipeID)
        {
            var recipeData = RecipesManager.Instance.Recipes[recipeID];
            /*if (EconomyEntities.Any(x => x.Production.RecipeData == recipeData))
            {
                Debug.LogWarning("EconomyEntity with this recipe already exists");
                return null;
            }*/

            var entity = EcomomyEntityManager.Instance.AddEconomyEntity(this, recipeData);
            entity.Production.Scale = 1;
            EconomyEntities.Add(entity);
            return entity;
        }

        public Pop AddPop(int recipeDataID)
        {
            var recipeData = RecipesManager.Instance.Recipes[recipeDataID];
            /*if (Pops.Any(x => x.Production.RecipeData == recipeData))
            {
                Debug.LogWarning("EconomyEntity with this recipe already exists");
                return null;
            }*/

            var pop = PopManager.Instance.AddPop(this, recipeData);
            pop.Production.Scale = 1;
            Pops.Add(pop);
            return pop;
        }
    }
}