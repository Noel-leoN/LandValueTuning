
using HarmonyLib;
using Game.Buildings;
using Game.City;
using Game.Economy;
using Game.Prefabs;
using AreaType = Game.Zones.AreaType;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Game.Simulation;
using Game.Areas;
using Game.Citizens;
using Game.Companies;
using Game.Net;
using Game.Objects;
using System;
using Unity.Collections;
using Game.Vehicles;

namespace LandValueTuning.Patches
{

    // This example patch adds the loading of a custom ECS System after the AudioManager has
    // its "OnGameLoadingComplete" method called. We're just using it as a entrypoint, and
    // it won't affect anything related to audio.
    /*[HarmonyPatch(typeof(AudioManager), "OnGameLoadingComplete")]
    class AudioManager_OnGameLoadingComplete
    {
        static void Postfix(AudioManager __instance, Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            if (!mode.IsGameOrEditor())
                return;

            // Here we add our custom ECS System to the game's ECS World, so it's "online" at runtime
            __instance.World.GetOrCreateSystem<LandValueTuningSystem>();
        }
    }*/

    // This example patch enables the editor in the main menu
    /*[HarmonyPatch(typeof(MenuUISystem), "IsEditorEnabled")]
    class MenuUISystem_IsEditorEnabledPatch
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;

            return false; // Ignore original function
        }
    }*/
    // Thanks to @89pleasure for the MenuUISystem_IsEditorEnabledPatch snippet above
    // https://github.com/89pleasure/cities2-mod-collection/blob/71385c000779c23b85e5cc023fd36022a06e9916/EditorEnabled/Patches/MenuUISystemPatches.cs

     //Its worked;Make Adjustable in the future;
    
    //Its worked! use it for more challenge.
    /*
    [HarmonyPatch(typeof(BuildingUtils), "GetLevelingCost")]
    public class BuildingUtils_GetLevelingCostPatch
    {
        public static bool Prefix(ref int __result, AreaType areaType, BuildingPropertyData propertyData, int currentlevel, DynamicBuffer<CityModifier> cityEffects)
        {
            //"capacity" of companies or rents for every building,res=m_ResidentialProperties，com=1,ind=1,else=0;
            int num = propertyData.CountProperties();
            //levelingup cost;
            float num2 = 0;
            
            //define levelingup cost multiple factor for each zonetype;            
            float resf = 2f;
            float commf =4f;
            float manufacturf =4f;
            float officef = 32f;
            float extractorf = 2f;
            float storagef = 4f;
            switch (areaType)
            {
                case AreaType.Residential:
                    num2 = (currentlevel <= 4) ? (resf * num * Mathf.RoundToInt(math.pow(2f, 2 * currentlevel) * 40f)) : 1073741823;
                    break;
                case AreaType.Commercial:
                    //商业；
                    num2 = (currentlevel <= 4) ? (commf * num * Mathf.RoundToInt(math.pow(2f, 2 * currentlevel) * 160f)) : 1073741823;
                    break;
                case AreaType.Industrial:
                    //num2 = ((currentlevel <= 4) ? (num * Mathf.RoundToInt(math.pow(2f, 2 * currentlevel) * 160f)) : 1073741823);
                    //vanila：住宅5级以下，1->2级=1倍（1*4*40=160），2->3级=4倍（4*4*40=640），3->4级=16倍（16*4*40=2560），4->5级=64倍（64*4*40=10240）；
                    //工商办5级以下，1->2级=1倍（1*4*160=640），2->3级=4倍（4*4*160=2560），3->4级=16倍（16*4*160=10240），4->5级=64倍（64*4*160=40960）；
                    //5级以上：+int32
                    //
                    //将工业细分为制造业、办公、采集、仓储；
                    //
                    if (propertyData.m_AllowedManufactured != Resource.NoResource)
                    {
                        if (EconomyUtils.IsOfficeResource(propertyData.m_AllowedManufactured))
                        {
                            //办公;
                            num2 = (currentlevel <= 4) ? (officef * num * Mathf.RoundToInt(math.pow(2f, 2 * currentlevel) * 160f)) : 1073741823;
                        }
                        else if (EconomyUtils.IsExtractorResource(propertyData.m_AllowedManufactured))
                        {
                            //采集业;
                            num2 = (currentlevel <= 4) ? (extractorf * num * Mathf.RoundToInt(math.pow(2f, 2 * currentlevel) * 160f)) : 1073741823;
                        }
                        else
                        {
                            //制造业;
                            num2 = (currentlevel <= 4) ? (manufacturf * num * Mathf.RoundToInt(math.pow(2f, 2 * currentlevel) * 160f)) : 1073741823;
                        }
                    }
                    if (propertyData.m_AllowedStored != Resource.NoResource)
                    {
                        //仓储业;
                        num2 = (currentlevel <= 4) ? (storagef * num * Mathf.RoundToInt(math.pow(2f, 2 * currentlevel) * 160f)) : 1073741823;
                        //num2 *= 4f; //vanila storage;  
                    }
                                        
                    break;
                case AreaType.None:
                    break;
                default:
                    num2 = 1.07374182E+09f;
                    break;
            }
            //add globle effects(e.x. signiture building effect);
            CityUtils.ApplyModifier(ref num2, cityEffects, CityModifierType.BuildingLevelingCost); 
            __result = Mathf.RoundToInt(num2);
            return false;
        }
    }*/


    //Its worked;must slight reduced even used RealEco. Make Adjustable in the future;
    [HarmonyPatch(typeof(PropertyRenterSystem), "GetUpkeep")]
    public class PropertyRenterSystem_GetUpkeepPatch
    {
        public static bool Prefix(ref int __result, int level, float baseUpkeep, int lotSize, AreaType areaType, bool isStorage = false)

        {
            int upkeep;
            float resfactor = 0.8f;//res upkeep reduce(or increase) factor;
            float otherfactor = 1f;//may subdivide for industry type;
            if (areaType == AreaType.Residential)
            {
                upkeep = Mathf.RoundToInt(math.pow(level, PropertyRenterSystem.GetUpkeepExponent(AreaType.Residential)) * baseUpkeep * (float)lotSize * resfactor);
            }
            else
            { 
                upkeep = Mathf.RoundToInt(math.pow(level, PropertyRenterSystem.GetUpkeepExponent(areaType)) * baseUpkeep * lotSize * (isStorage ? 0.5f : 1f) * otherfactor); 
            }
            __result = upkeep;
            //return true;
            return false;
        }
    }

    //Looking for a better way to patch int2 result;Make Adjustable in the future;(or its no necessary)
    //[HarmonyPatch(typeof(RentAdjustSystem), "GetRent")]
    //public class RentAdjustSystem_GetRentPatch
    //{
      //  public static int2 Postfix(ref int2 __result)//int2 rent
                                   ///*ConsumptionData consumptionData, BuildingPropertyData buildingProperties, float landValue, global::Game.Zones.AreaType areaType*/
       // {
            //float2 @float = default(float2);
            //@float.x = (float)consumptionData.m_Upkeep / PropertyRenterSystem.GetUpkeepExponent(areaType);
            //@float.y = @float.x;
            //float num;
            //num = ((buildingProperties.m_ResidentialProperties <= 0 || (buildingProperties.m_AllowedSold == Resource.NoResource && buildingProperties.m_AllowedManufactured == Resource.NoResource)) ? ((float)buildingProperties.CountProperties()) : ((float)Mathf.RoundToInt((float)buildingProperties.m_ResidentialProperties / (1f - RentAdjustSystem.kMixedCompanyRent))));
            //@float.x += math.max(0f, 1f * landValue);
            //@float /= num;
            //return new int2(Mathf.RoundToInt(@float.x), Mathf.RoundToInt(@float.y));
            //int numx;
            //int numy;
            //if (buildingProperties.m_ResidentialProperties <= 0 || (buildingProperties.m_AllowedSold == Resource.NoResource && buildingProperties.m_AllowedManufactured == Resource.NoResource))

            //if (buildingProperties.m_ResidentialProperties > 0 || (buildingProperties.m_AllowedSold == Resource.NoResource && buildingProperties.m_AllowedManufactured == Resource.NoResource))
            //{
            //   numx = Mathf.RoundToInt(__result.x * 2f);
            //    numy = Mathf.RoundToInt(__result.y * 2f);
            //   __result = new int2(numx, numy);
            // }
            //else
            // {
            //     numx = __result.x;
            //    numy = __result.y;
            //    __result = new int2(numx, numy);
            // }
            
       //     return __result = new int2 (Mathf.RoundToInt(__result.x * 0.1f), Mathf.RoundToInt(__result.y * 0.1f));           

       //}
   // }

    //It has no effect now;住宅分区生成建筑适用性分数，越低则越容易长出建筑(铺设低密分区时道路难以变红)；
    /*[HarmonyPatch(typeof(ZoneEvaluationUtils), "GetResidentialScore")]
    public class ZoneEvaluationUtils_GetResidentialScore_Patch
    {
        public static bool Prefix(ref float __result, DynamicBuffer<ResourceAvailability> availabilities, float curvePos, ref ZonePreferenceData preferences, float landValue, float2 pollution)
        {
            float num = 555f + (((0f - preferences.m_ResidentialSignificanceServices) / math.max(0.1f, NetUtils.GetAvailability(availabilities, AvailableResource.Services, curvePos))) - (preferences.m_ResidentialSignificanceWorkplaces / math.max(0.1f, NetUtils.GetAvailability(availabilities, AvailableResource.Workplaces, curvePos))) + ((preferences.m_ResidentialSignificancePollution * pollution.x) + (preferences.m_ResidentialSignificancePollutionDelta * pollution.y)) + (preferences.m_ResidentialSignificanceLandValue * ( landValue - preferences.m_ResidentialNeutralLandValue)));
            __result = num;
            return false;
        }
    }*/


    //MaxRent adjustment;?;
    /*[HarmonyPatch(typeof(RentAdjustSystem), "CalculateMaximumRent")]
    public class RentAdjustSystem_CalculateMaximumRentPatch
    {
        public static int2 Postfix(ref int2 __result, Entity renter, ref EconomyParameterData economyParameters, ref DemandParameterData demandParameters, float baseConsumptionSum, DynamicBuffer<CityModifier> cityModifiers, PropertyRenter propertyRenter, Entity healthcareService, Entity entertainmentService, Entity educationService, Entity telecomService, Entity garbageService, Entity policeService, ref ComponentLookup<Household> households, ref ComponentLookup<Worker> workers, ref ComponentLookup<Building> buildings, ref BufferLookup<HouseholdCitizen> householdCitizens, ref ComponentLookup<PrefabRef> prefabs, ref BufferLookup<ResourceAvailability> availabilities, ref ComponentLookup<BuildingPropertyData> buildingProperties, ref ComponentLookup<BuildingData> buildingDatas, ref ComponentLookup<SpawnableBuildingData> spawnableBuildings, ref ComponentLookup<CrimeProducer> crimes, ref BufferLookup<global::Game.Net.ServiceCoverage> serviceCoverages, ref ComponentLookup<Locked> locked, ref ComponentLookup<ElectricityConsumer> electricityConsumers, ref ComponentLookup<AdjustHappinessData> adjustHappinessDatas, ref ComponentLookup<WaterConsumer> waterConsumers, ref ComponentLookup<GarbageProducer> garbageProducers, ref ComponentLookup<MailProducer> mailProducers, ref ComponentLookup<global::Game.Objects.Transform> transforms, NativeArray<GroundPollution> pollutionMap, NativeArray<AirPollution> airPollutionMap, NativeArray<NoisePollution> noiseMap, CellMapData<TelecomCoverage> telecomCoverages, ResourcePrefabs resourcePrefabs, ref ComponentLookup<ResourceData> resourceDatas, ref ComponentLookup<IndustrialProcessData> processDatas, ref ComponentLookup<global::Game.Companies.StorageCompany> storageCompanies, ref ComponentLookup<ServiceAvailable> serviceAvailables, ref ComponentLookup<WorkProvider> workProviders, ref ComponentLookup<ServiceCompanyData> serviceCompanyDatas, ref ComponentLookup<WorkplaceData> workplaceDatas, ref ComponentLookup<global::Game.Companies.ProcessingCompany> processingCompanies, ref ComponentLookup<BuyingCompany> buyingCompanies, ref BufferLookup<global::Game.Areas.SubArea> subAreas, ref ComponentLookup<Attached> attached, ref ComponentLookup<global::Game.Areas.Lot> lots, ref ComponentLookup<Geometry> geometries, ref ComponentLookup<Extractor> areaExtractors, ref ComponentLookup<HealthProblem> healthProblems, CitizenHappinessParameterData happinessParameterData, GarbageParameterData garbageParameterData, NativeArray<int> taxRates, ref ComponentLookup<CurrentDistrict> districts, ref BufferLookup<DistrictModifier> districtModifiers, ref BufferLookup<Employee> employees, ref BufferLookup<Efficiency> buildingEfficiencies, ref ComponentLookup<ExtractorAreaData> extractorDatas, ExtractorParameterData extractorParameters, ref ComponentLookup<Citizen> citizenDatas, ref ComponentLookup<global::Game.Citizens.Student> students, NativeArray<int> unemployment, ref BufferLookup<TradeCost> tradeCosts, ref ComponentLookup<Abandoned> abandoneds)
        {
            Entity property = propertyRenter.m_Property;
            int2 @int = default;
            if (households.HasComponent(renter))
            {
                float commuteTime = 0f;
                if (workers.HasComponent(renter))
                {
                    commuteTime = workers[renter].m_LastCommuteTime;
                }
                Building buildingData = buildings[property];
                DynamicBuffer<HouseholdCitizen> dynamicBuffer = householdCitizens[renter];
                int length = dynamicBuffer.Length;
                int num = 0;//全家幸福度；
                int num2 = 0;//家中成人幸福度；
                int num3 = 0;//家中儿童幸福度；
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                    Entity citizen = dynamicBuffer[i].m_Citizen;
                    Citizen citizen2 = citizenDatas[citizen];
                    num += (citizenDatas.HasComponent(citizen) ? citizenDatas[citizen].Happiness : 50);
                    if (citizen2.GetAge() == CitizenAge.Child)
                    {
                        num3++;
                    }
                    else
                    {
                        num2 += CitizenHappinessSystem.GetTaxBonuses(citizen2.GetEducationLevel(), taxRates, in happinessParameterData).y;
                    }
                }
                num /= math.max(1, dynamicBuffer.Length);
                num2 /= math.max(1, dynamicBuffer.Length - num3);
                Entity prefab = prefabs[property].m_Prefab;
                float shoppingTime = HouseholdFindPropertySystem.EstimateShoppingTime(buildingData.m_RoadEdge, buildingData.m_CurvePosition, hasCar: true, availabilities);
                float apartmentQuality = HouseholdFindPropertySystem.GetApartmentQuality(dynamicBuffer.Length, num3, property, ref buildingData, prefab, ref buildingProperties, ref buildingDatas, ref spawnableBuildings, ref crimes, ref serviceCoverages, ref locked, ref electricityConsumers, ref adjustHappinessDatas, ref waterConsumers, ref garbageProducers, ref mailProducers, ref prefabs, ref transforms, ref abandoneds, pollutionMap, airPollutionMap, noiseMap, telecomCoverages, cityModifiers, healthcareService, entertainmentService, educationService, telecomService, garbageService, policeService, happinessParameterData, garbageParameterData, num);
                int householdIncome = HouseholdBehaviorSystem.GetHouseholdIncome(dynamicBuffer, ref workers, ref citizenDatas, ref healthProblems, ref economyParameters, taxRates);
                int mrres = Math.Max(0, Math.Min(
                    val2: HouseholdFindPropertySystem.FindRentToProvideUtility(
                    income2: HouseholdBehaviorSystem.GetHouseholdExpectedIncome(dynamicBuffer, ref students, ref healthProblems, ref citizenDatas, ref economyParameters, taxRates, unemployment),
                    target: HouseholdFindPropertySystem.EvaluateDefaultProperty(HouseholdBehaviorSystem.GetHouseholdIncomeDefaultTax(dynamicBuffer, ref workers, ref healthProblems, ref citizenDatas, ref economyParameters), HouseholdBehaviorSystem.GetHouseholdExpectedIncomeDefault(dynamicBuffer, ref students, ref healthProblems, ref citizenDatas, ref economyParameters), length, HouseholdBehaviorSystem.GetHighestEducation(dynamicBuffer, ref citizenDatas), ref economyParameters, ref demandParameters, resourcePrefabs, resourceDatas, baseConsumptionSum, happinessParameterData, bonus: false, print: false, out var _, out var _),
                    familySize: length, income: householdIncome, commuteTime: commuteTime, shoppingTime: shoppingTime,
                    quality: apartmentQuality + num2 / 2f, economyParameters: ref economyParameters,
                    resourcePrefabs: resourcePrefabs, resourceDatas: resourceDatas,
                    baseConsumptionSum: baseConsumptionSum, happinessParameterData: happinessParameterData), 
                    val1: Mathf.RoundToInt(0.35f * householdIncome)));//val1: Mathf.RoundToInt(0.45f * householdIncome)
                @int.x = mrres;
                @int.y = mrres;               
            }
            Entity prefab2 = prefabs[renter].m_Prefab;
            Entity prefab3 = prefabs[property].m_Prefab;
            IndustrialProcessData processData = processDatas[prefab2];
            float efficiency = BuildingUtils.GetEfficiency(property, ref buildingEfficiencies);
            if (storageCompanies.HasComponent(renter))//仓储；
            {
                @int.x = 0;
                @int.y = 0;
            }
            else if (serviceAvailables.HasComponent(renter))
            {
                WorkProvider workProvider = workProviders[renter];
                ServiceAvailable service = serviceAvailables[renter];
                ServiceCompanyData serviceData = serviceCompanyDatas[prefab2];
                WorkplaceData workplaceData = workplaceDatas[prefab2];
                DynamicBuffer<Employee> employees2 = employees[renter];
                DynamicBuffer<TradeCost> tradeCosts2 = tradeCosts[renter];
                BuildingData buildingData2 = buildingDatas[prefab3];
                SpawnableBuildingData spawnableData = spawnableBuildings[prefab3];
                int fittingWorkers = CommercialAISystem.GetFittingWorkers(buildingDatas[prefab3], buildingProperties[prefab3], spawnableData.m_Level, serviceData);
                @int.x = Mathf.RoundToInt(ServiceCompanySystem.EstimateDailyProfit(efficiency, workProvider.m_MaxWorkers, employees2, service, serviceData, buildingData2, processData, ref economyParameters, workplaceData, spawnableData, resourcePrefabs, resourceDatas, tradeCosts2));
                @int.y = math.max(@int.x, Mathf.RoundToInt(ServiceCompanySystem.EstimateDailyProfitFull(1f, fittingWorkers, service, serviceData, buildingData2, processData, ref economyParameters, workplaceData, spawnableData, resourcePrefabs, resourceDatas, tradeCosts2)));
                int num6 = (!districts.HasComponent(property)) ? TaxSystem.GetCommercialTaxRate(processData.m_Output.m_Resource, taxRates) : TaxSystem.GetModifiedCommercialTaxRate(district: districts[property].m_District, resource: processData.m_Output.m_Resource, taxRates: taxRates, policies: districtModifiers);
                float taxfc = 1f - (num6 / 100f);
                taxfc *= 0.8f;//最大租金-商业-影响因子；
                @int.x = Mathf.RoundToInt(@int.x * taxfc);
                @int.y = Mathf.RoundToInt(@int.y * taxfc);
            }
            else if (processingCompanies.HasComponent(renter))
            {
                WorkProvider workProvider2 = workProviders[renter];
                if (buyingCompanies.HasComponent(renter) && tradeCosts.HasBuffer(renter))
                {
                    SpawnableBuildingData building = spawnableBuildings[prefab3];
                    DynamicBuffer<Employee> employees3 = employees[renter];
                    int fittingWorkers2 = IndustrialAISystem.GetFittingWorkers(buildingDatas[prefab3], buildingProperties[prefab3], building.m_Level, processData);
                    WorkplaceData workplaceData2 = workplaceDatas[prefab2];
                    @int.x = Mathf.RoundToInt(ProcessingCompanySystem.EstimateDailyProfit(employees3, efficiency, workProvider2, processData, ref economyParameters, tradeCosts[renter], workplaceData2, building, resourcePrefabs, resourceDatas));
                    @int.y = Mathf.RoundToInt(ProcessingCompanySystem.EstimateDailyProfitFull(1f, fittingWorkers2, processData, ref economyParameters, tradeCosts[renter], workplaceData2, building, resourcePrefabs, resourceDatas));
                    ResourceData resourceData = resourceDatas[resourcePrefabs[processData.m_Output.m_Resource]];
                    bool isoffice = resourceData.m_Weight == 0f;
                    float taxfi;
                    if (isoffice)
                    {
                        taxfi = 1f - (TaxSystem.GetOfficeTaxRate(processData.m_Output.m_Resource, taxRates) / 100f);
                        taxfi *= 0.5f;//办公因子；
                    }
                    else
                    {
                        taxfi = 1f - (float)TaxSystem.GetIndustrialTaxRate(processData.m_Output.m_Resource, taxRates) / 100f;
                        taxfi *= 0.8f;//制造业因子；
                    }
                    //@int.x = Mathf.RoundToInt(@int.x * taxfi);
                    //@int.y = math.max(@int.x, Mathf.RoundToInt(@int.y * taxfi));
                    @int.x = 27;
                    @int.y = 27;
                }
                else if (attached.HasComponent(property))
                {
                    DynamicBuffer<Employee> employees4 = employees[renter];
                    WorkplaceData workplaceData3 = workplaceDatas[prefab2];
                    SpawnableBuildingData building2 = spawnableBuildings[prefab3];
                    @int.x = Mathf.RoundToInt(ExtractorCompanySystem.EstimateDailyProfit(ExtractorCompanySystem.EstimateDailyProduction(efficiency, workProvider2.m_MaxWorkers, building2.m_Level, workplaceData3, processData, ref economyParameters), employees4, processData, ref economyParameters, workplaceData3, building2, resourcePrefabs, resourceDatas) * (1f - (TaxSystem.GetIndustrialTaxRate(processData.m_Output.m_Resource, taxRates) / 100f)));
                    @int.y = ExtractorCompanySystem.EstimateDailyProfitFull(ExtractorCompanySystem.EstimateDailyProduction(1f, workProvider2.m_MaxWorkers, building2.m_Level, workplaceData3, processData, ref economyParameters), processData, ref economyParameters, workplaceData3, building2, resourcePrefabs, resourceDatas);
                    float taxfe = 1f;//采集业因子；
                    @int.x = Mathf.RoundToInt(@int.x * taxfe);
                    @int.y = Mathf.RoundToInt(@int.y * taxfe);
                }
                else
                {
                    @int.x = 0;
                }
            }

            return __result = new int2(1666,1666);
            //return __result = new int2(Math.Max(0, @int.x), @int.y);
            //return false;
            //return true;
        }
    }
    }*/
    

}