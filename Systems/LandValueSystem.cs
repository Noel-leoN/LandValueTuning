using System.Runtime.CompilerServices;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Economy;
using Game.Net;
using Game.Objects;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

namespace LandValueTuning.Systems;


[CompilerGenerated]
public class LandValueSystemMod : GameSystemBase
{
    [BurstCompile]
    private struct EdgeUpdateJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public BufferTypeHandle<ConnectedBuilding> m_ConnectedBuildingType;

        [ReadOnly]
        public ComponentTypeHandle<Edge> m_EdgeType;

        [ReadOnly]
        public ComponentTypeHandle<Curve> m_CurveType;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<LandValue> m_LandValues;

        [ReadOnly]
        public BufferLookup<Renter> m_RenterBuffers;

        [ReadOnly]
        public ComponentLookup<global::Game.Objects.Transform> m_Transforms;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> m_PropertyRenters;

        [ReadOnly]
        public ComponentLookup<PrefabRef> m_Prefabs;

        [ReadOnly]
        public ComponentLookup<BuildingData> m_BuildingDatas;

        [ReadOnly]
        public ComponentLookup<Abandoned> m_Abandoneds;

        [ReadOnly]
        public ComponentLookup<Destroyed> m_Destroyeds;

        [ReadOnly]
        public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> m_PropertyDatas;

        [ReadOnly]
        public ComponentLookup<Household> m_Households;

        [ReadOnly]
        public ComponentLookup<Placeholder> m_Placeholders;

        [ReadOnly]
        public ComponentLookup<Attached> m_Attached;

        [ReadOnly]
        public BufferLookup<global::Game.Areas.SubArea> m_SubAreas;

        [ReadOnly]
        public ComponentLookup<global::Game.Areas.Lot> m_Lots;

        [ReadOnly]
        public ComponentLookup<Geometry> m_Geometries;

        [ReadOnly]
        public NativeArray<GroundPollution> m_PollutionMap;

        [ReadOnly]
        public PollutionParameterData m_PollutionParameters;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray;
            nativeArray = chunk.GetNativeArray(this.m_EntityType);
            NativeArray<Edge> nativeArray2;
            nativeArray2 = chunk.GetNativeArray(ref this.m_EdgeType);
            NativeArray<Curve> nativeArray3;
            nativeArray3 = chunk.GetNativeArray(ref this.m_CurveType);
            BufferAccessor<ConnectedBuilding> bufferAccessor;
            bufferAccessor = chunk.GetBufferAccessor(ref this.m_ConnectedBuildingType);
            for (int i = 0; i < nativeArray2.Length; i++)
            {
                Entity entity;
                entity = nativeArray[i];
                Entity start;
                start = nativeArray2[i].m_Start;
                Entity end;
                end = nativeArray2[i].m_End;
                LandValue value;
                value = this.m_LandValues[entity];
                int num;
                num = 0;
                float num2;
                num2 = 0f;
                int num3;
                num3 = 0;
                float num4;
                num4 = 0f;
                DynamicBuffer<ConnectedBuilding> dynamicBuffer;
                dynamicBuffer = bufferAccessor[i];
                for (int j = 0; j < dynamicBuffer.Length; j++)
                {
                    Entity building;
                    building = dynamicBuffer[j].m_Building;
                    if (this.m_Prefabs.HasComponent(building) && !this.m_Placeholders.HasComponent(building))
                    {
                        Entity prefab;
                        prefab = this.m_Prefabs[building].m_Prefab;
                        if (!this.m_PropertyDatas.HasComponent(prefab) || this.m_Abandoneds.HasComponent(building) || this.m_Destroyeds.HasComponent(building))
                        {
                            continue;
                        }
                        BuildingPropertyData buildingPropertyData;
                        buildingPropertyData = this.m_PropertyDatas[prefab];
                        if (buildingPropertyData.m_AllowedStored != Resource.NoResource)
                        {
                            continue;
                        }
                        BuildingData buildingData;
                        buildingData = this.m_BuildingDatas[prefab];
                        ConsumptionData consumptionData;
                        consumptionData = this.m_ConsumptionDatas[prefab];
                        int num5;
                        num5 = buildingPropertyData.CountProperties();
                        bool flag;
                        flag = buildingPropertyData.m_ResidentialProperties > 0 && (buildingPropertyData.m_AllowedSold != Resource.NoResource || buildingPropertyData.m_AllowedManufactured != Resource.NoResource);
                        int num6;
                        num6 = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                        if (this.m_Attached.HasComponent(building))
                        {
                            Entity parent;
                            parent = this.m_Attached[building].m_Parent;
                            if (this.m_SubAreas.HasBuffer(parent))
                            {
                                num6 += Mathf.CeilToInt(ExtractorAISystem.GetArea(this.m_SubAreas[parent], this.m_Lots, this.m_Geometries));
                            }
                        }
                        float num7;
                        num7 = value.m_LandValue * (float)num6 / (float)math.max(1, num5);
                        float num8;
                        num8 = (float)consumptionData.m_Upkeep / (float)math.max(1, num5);
                        if (this.m_RenterBuffers.HasBuffer(building))
                        {
                            DynamicBuffer<Renter> dynamicBuffer2;
                            dynamicBuffer2 = this.m_RenterBuffers[building];
                            for (int k = 0; k < dynamicBuffer2.Length; k++)
                            {
                                Entity renter;
                                renter = dynamicBuffer2[k].m_Renter;
                                if (this.m_PropertyRenters.HasComponent(renter))
                                {
                                    PropertyRenter propertyRenter;
                                    propertyRenter = m_PropertyRenters[renter];
                                    //num2地价更新因素:建筑最大租金-维护费，若余额大于3倍当前地价则增加1，反之减少1；
                                    //其中混合建筑维护费和地价按0.4计算,增减量按0.4倍户数计算；
                                    //计算每户num2得到累计值；
                                    //New feature to calculate landvalue separately for each zonetype by simply change the result by modify MaxRent reference;
                                    //method may extened by cap the max landvalue; 
                                    //Avoid to modifiy MaxRent method directly for better compatibility;
                                    float resmixf = 1.2f;//住商住工混合; 
                                    if (!flag || m_Households.HasComponent(renter))
                                    {
                                        //if (m_Households.HasComponent(renter))
                                        //{
                                        //num2 = (float)(num2 + (propertyRenter.m_MaxRent - num8 >= 3f * num7 ? 1f : -1f));
                                        
                                        //***suit for RealEco default Prefabs***//
                                        //***need more test to get more suitable***// 
                                        float residentlowf = 0.8f;//低密住宅（1户）
                                        float residenthighf = 0.6f;//中高密住宅;(不含住商住工混合);
                                        float commerialf = 0.6f;//商业；
                                        float manufacturf = 2f;//制造业；1.19版增加土地污染用力过猛导致工业地价过低；
                                        float officef = 0.45f;//办公；
                                        float extractorf = 1f;//采集业；                                                                       
                                        //float storagef = 1f;//仓储业；
                                                                                
                                        //低密住宅（资产属性值为1）;调低以抑制地价提升适用度；
                                        if (buildingPropertyData.m_ResidentialProperties == 1)
                                        {
                                            num2 = (float)(num2 + ((propertyRenter.m_MaxRent * residentlowf - num8 >= 3f * num7) ? 1f : (-1f)));
                                        }
                                    //中高密住宅（不含住商混）；略微调低以抑制高租金住宅地价；
                                        if (buildingPropertyData.m_ResidentialProperties > 0 && (buildingPropertyData.m_AllowedSold == Resource.NoResource || buildingPropertyData.m_AllowedManufactured == Resource.NoResource))
                                        {
                                            num2 = (float)(num2 + ((propertyRenter.m_MaxRent * residenthighf - num8 >= 3f * num7) ? 1f : (-1f)));
                                        }
                                        //商业；略微调低以抑制高利润商业地价；
                                        if (buildingPropertyData.m_AllowedSold != Resource.NoResource && buildingPropertyData.m_ResidentialProperties <= 0)
                                        {
                                            num2 = (float)(num2 + ((propertyRenter.m_MaxRent * commerialf - num8 >= 3f * num7) ? 1f : (-1f)));
                                        }
                                        //工业；
                                        if (buildingPropertyData.m_AllowedManufactured != Resource.NoResource && buildingPropertyData.m_ResidentialProperties <= 0)
                                        //IndustrialProcessData process = this.m_IndustrialProcessDatas[prefab];
                                        //办公；大幅调低以抑制超高利润办公地价；may not suitable for RealEco；
                                        {
                                            if (EconomyUtils.IsOfficeResource(buildingPropertyData.m_AllowedManufactured))
                                            {
                                                num2 = (float)(num2 + ((propertyRenter.m_MaxRent * officef - num8 >= 3f * num7) ? 1f : (-1f)));
                                            }
                                            //采集业；地价影响不大；
                                            if (EconomyUtils.IsExtractorResource(buildingPropertyData.m_AllowedManufactured))
                                            {
                                                num2 = (float)(num2 + ((propertyRenter.m_MaxRent * extractorf - num8 >= 3f * num7) ? 1f : (-1f)));
                                            }
                                            //仓储业；
                                            //if(buildingPropertyData.m_AllowedSold != Resource.NoResource)
                                            //{
                                            //    num2 = 0f;
                                            //}
                                            //制造业；地价受污染影响过大，谨慎调整避免过低；
                                            else
                                            {
                                                num2 = (float)(num2 + ((propertyRenter.m_MaxRent * manufacturf - num8 >= 3f * num7) ? 1f : (-1f)));
                                            }
                                        }
                                    }
                                    else//住商住工混合；
                                    {
                                        //num2 = (float)(num2 + (propertyRenter.m_MaxRent * resmixf - RentAdjustSystem.kMixedCompanyRent * consumptionData.m_Upkeep >= 3f * RentAdjustSystem.kMixedCompanyRent * value.m_LandValue ? Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * buildingPropertyData.m_ResidentialProperties) : -Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * buildingPropertyData.m_ResidentialProperties)));
                                        num2 = (float)(num2 + ((propertyRenter.m_MaxRent * resmixf - RentAdjustSystem.kMixedCompanyRent * consumptionData.m_Upkeep >= 3f * RentAdjustSystem.kMixedCompanyRent * value.m_LandValue) ? Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * buildingPropertyData.m_ResidentialProperties) : (-Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * buildingPropertyData.m_ResidentialProperties))));
                                    }
                                    //}
                                    //***计算租户总数num3；
                                    num3++;
                                }
                            }
                            num += num6;
                            int num9;
                            num9 = num5 - dynamicBuffer2.Length;
                            num2 -= (float)num9;
                            num3 += num9;
                        }
                        if (this.m_Transforms.HasComponent(building))
                        {
                            num4 = (float)GroundPollutionSystem.GetPollution(this.m_Transforms[building].m_Position, this.m_PollutionMap).m_Pollution / (float)this.m_PollutionParameters.m_GroundPollutionLandValueDivisor;
                        }
                    }
                    else
                    {
                        num++;
                    }
                }
                float length;
                length = nativeArray3[i].m_Length;
                float distanceFade;
                distanceFade = LandValueSystemMod.GetDistanceFade(length);
                int num10;
                //num10 = num;
                num10 = math.max(num, Mathf.CeilToInt(length / 4f));//
                //num3 -= num - num10;
                num3 -= num - num10;//vanilla定义密集建筑路段与稀疏路段的影响,但弄反了, its ridiculous~

                //权重(接壤道路地价)影响当前道路地价计算；
                float2 @float;
                @float = new float2(math.max(1f, this.m_LandValues[start].m_Weight), math.max(1f, this.m_LandValues[end].m_Weight));
                float num11;
                num11 = @float.x + @float.y;
                float2 float2;
                float2 = new float2(this.m_LandValues[start].m_LandValue, this.m_LandValues[end].m_LandValue);
                //@float *= distanceFade;
                @float *= distanceFade;//both x & y,but x is not used;so maybe they considered this but forgot later;
                float y;
                y= 0f;
                //y = math.lerp(float2.x, float2.y, @float.y / num11);//vanilla;only defined start.weight<end.weight,not oppisite,so ridiculous:-)
                if (float2.y >= float2.x)//Fix;
                {
                    y = math.lerp(float2.x, float2.y, @float.y / num11);
                }
                if (float2.y < float2.x)//Fix;
                {
                    y = math.lerp(float2.y, float2.x, @float.x / num11);
                }                

                float num12;
                num12 = 0f;
                if (num3 > 0)
                {
                    num12 = 0.1f * num2 / (float)num3;
                }
                if (num4 > 0f)
                {
                    num4 = math.lerp(0f, 2f, num4 / 50f);
                }
                
                value.m_Weight = math.max(1f, math.lerp(value.m_Weight, num10, 0.1f));//
               
                float s;
                s = num11 / (99f * value.m_Weight + num11);
                value.m_LandValue = math.lerp(value.m_LandValue, y, s);
                if (value.m_LandValue > 30f)
                {
                    num12 -= num4 * 0.2f;
                }
                value.m_LandValue += math.min(1f, math.max(-2f, num12));
                value.m_LandValue = math.max(value.m_LandValue, 0f);
                value.m_Weight = math.lerp(value.m_Weight, math.max(1f, 0.5f * num11), s);
                this.m_LandValues[entity] = value;
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            this.Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
        }
    }

    [BurstCompile]
    private struct NodeUpdateJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<global::Game.Net.Node> m_NodeType;

        [ReadOnly]
        public BufferTypeHandle<ConnectedEdge> m_ConnectedEdgeType;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<LandValue> m_LandValues;

        [ReadOnly]
        public ComponentLookup<Curve> m_Curves;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray;
            nativeArray = chunk.GetNativeArray(this.m_EntityType);
            NativeArray<global::Game.Net.Node> nativeArray2;
            nativeArray2 = chunk.GetNativeArray(ref this.m_NodeType);
            BufferAccessor<ConnectedEdge> bufferAccessor;
            bufferAccessor = chunk.GetBufferAccessor(ref this.m_ConnectedEdgeType);
            for (int i = 0; i < nativeArray2.Length; i++)
            {
                Entity entity;
                entity = nativeArray[i];
                float num;
                num = 0f;
                float num2;
                num2 = 0f;
                DynamicBuffer<ConnectedEdge> dynamicBuffer;
                dynamicBuffer = bufferAccessor[i];
                for (int j = 0; j < dynamicBuffer.Length; j++)
                {
                    Entity edge;
                    edge = dynamicBuffer[j].m_Edge;
                    if (this.m_LandValues.HasComponent(edge))
                    {
                        float landValue;
                        landValue = this.m_LandValues[edge].m_LandValue;
                        float num3;
                        num3 = this.m_LandValues[edge].m_Weight;
                        if (this.m_Curves.HasComponent(edge))
                        {
                            num3 *= LandValueSystemMod.GetDistanceFade(this.m_Curves[edge].m_Length);
                        }
                        num += landValue * num3;
                        num2 += num3;
                    }
                }
                if (num2 != 0f)
                {
                    num /= num2;
                    LandValue value;
                    value = this.m_LandValues[entity];
                    value.m_LandValue = math.lerp(value.m_LandValue, num, 0.05f);
                    value.m_Weight = math.max(1f, math.lerp(value.m_Weight, num2 / (float)dynamicBuffer.Length, 0.05f));
                    this.m_LandValues[entity] = value;
                }
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            this.Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Edge> __Game_Net_Edge_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Curve> __Game_Net_Curve_RO_ComponentTypeHandle;

        [ReadOnly]
        public BufferTypeHandle<ConnectedBuilding> __Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<global::Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentLookup;

        public ComponentLookup<LandValue> __Game_Net_LandValue_RW_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<Renter> __Game_Buildings_Renter_RO_BufferLookup;

        [ReadOnly]
        public ComponentLookup<Abandoned> __Game_Buildings_Abandoned_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Destroyed> __Game_Common_Destroyed_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<ConsumptionData> __Game_Prefabs_ConsumptionData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Placeholder> __Game_Objects_Placeholder_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Attached> __Game_Objects_Attached_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<global::Game.Areas.SubArea> __Game_Areas_SubArea_RO_BufferLookup;

        [ReadOnly]
        public ComponentLookup<global::Game.Areas.Lot> __Game_Areas_Lot_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Geometry> __Game_Areas_Geometry_RO_ComponentLookup;

        [ReadOnly]
        public ComponentTypeHandle<global::Game.Net.Node> __Game_Net_Node_RO_ComponentTypeHandle;

        [ReadOnly]
        public BufferTypeHandle<ConnectedEdge> __Game_Net_ConnectedEdge_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentLookup<Curve> __Game_Net_Curve_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            this.__Game_Net_Edge_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Edge>(isReadOnly: true);
            this.__Game_Net_Curve_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Curve>(isReadOnly: true);
            this.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedBuilding>(isReadOnly: true);
            this.__Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(isReadOnly: true);
            this.__Game_Objects_Transform_RO_ComponentLookup = state.GetComponentLookup<global::Game.Objects.Transform>(isReadOnly: true);
            this.__Game_Net_LandValue_RW_ComponentLookup = state.GetComponentLookup<LandValue>();
            this.__Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(isReadOnly: true);
            this.__Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(isReadOnly: true);
            this.__Game_Buildings_Renter_RO_BufferLookup = state.GetBufferLookup<Renter>(isReadOnly: true);
            this.__Game_Buildings_Abandoned_RO_ComponentLookup = state.GetComponentLookup<Abandoned>(isReadOnly: true);
            this.__Game_Common_Destroyed_RO_ComponentLookup = state.GetComponentLookup<Destroyed>(isReadOnly: true);
            this.__Game_Prefabs_ConsumptionData_RO_ComponentLookup = state.GetComponentLookup<ConsumptionData>(isReadOnly: true);
            this.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
            this.__Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
            this.__Game_Objects_Placeholder_RO_ComponentLookup = state.GetComponentLookup<Placeholder>(isReadOnly: true);
            this.__Game_Objects_Attached_RO_ComponentLookup = state.GetComponentLookup<Attached>(isReadOnly: true);
            this.__Game_Areas_SubArea_RO_BufferLookup = state.GetBufferLookup<global::Game.Areas.SubArea>(isReadOnly: true);
            this.__Game_Areas_Lot_RO_ComponentLookup = state.GetComponentLookup<global::Game.Areas.Lot>(isReadOnly: true);
            this.__Game_Areas_Geometry_RO_ComponentLookup = state.GetComponentLookup<Geometry>(isReadOnly: true);
            this.__Game_Net_Node_RO_ComponentTypeHandle = state.GetComponentTypeHandle<global::Game.Net.Node>(isReadOnly: true);
            this.__Game_Net_ConnectedEdge_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedEdge>(isReadOnly: true);
            this.__Game_Net_Curve_RO_ComponentLookup = state.GetComponentLookup<Curve>(isReadOnly: true);
        }
    }

    private GroundPollutionSystem m_GroundPollutionSystem;

    private EntityQuery m_EdgeGroup;

    private EntityQuery m_NodeGroup;

    private EntityQuery m_PollutionParameterQuery;

    private TypeHandle __TypeHandle;

    public override int GetUpdateInterval(SystemUpdatePhase phase)
    {
        return 16;
    }

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        this.m_GroundPollutionSystem = base.World.GetOrCreateSystemManaged<GroundPollutionSystem>();
        this.m_PollutionParameterQuery = base.GetEntityQuery(ComponentType.ReadOnly<PollutionParameterData>());
        this.m_EdgeGroup = base.GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[4]
            {
                ComponentType.ReadOnly<Edge>(),
                ComponentType.ReadWrite<LandValue>(),
                ComponentType.ReadOnly<Curve>(),
                ComponentType.ReadOnly<ConnectedBuilding>()
            },
            Any = new ComponentType[0],
            None = new ComponentType[2]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
            }
        });
        this.m_NodeGroup = base.GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[3]
            {
                ComponentType.ReadOnly<global::Game.Net.Node>(),
                ComponentType.ReadWrite<LandValue>(),
                ComponentType.ReadOnly<ConnectedEdge>()
            },
            Any = new ComponentType[0],
            None = new ComponentType[2]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
            }
        });
        base.RequireAnyForUpdate(this.m_EdgeGroup, this.m_NodeGroup);
    }

    [Preserve]
    protected override void OnUpdate()
    {
        JobHandle jobHandle;
        jobHandle = base.Dependency;
        if (!this.m_EdgeGroup.IsEmptyIgnoreFilter)
        {
            this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Renter_RO_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Objects_Transform_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
            EdgeUpdateJob jobData;
            jobData = default(EdgeUpdateJob);
            jobData.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
            jobData.m_EdgeType = this.__TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle;
            jobData.m_CurveType = this.__TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle;
            jobData.m_ConnectedBuildingType = this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;
            jobData.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
            jobData.m_Transforms = this.__TypeHandle.__Game_Objects_Transform_RO_ComponentLookup;
            jobData.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
            jobData.m_Prefabs = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
            jobData.m_PropertyRenters = this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup;
            jobData.m_RenterBuffers = this.__TypeHandle.__Game_Buildings_Renter_RO_BufferLookup;
            jobData.m_Abandoneds = this.__TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup;
            jobData.m_Destroyeds = this.__TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup;
            jobData.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
            jobData.m_PropertyDatas = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
            jobData.m_Households = this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
            jobData.m_Placeholders = this.__TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup;
            jobData.m_Attached = this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup;
            jobData.m_SubAreas = this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup;
            jobData.m_Lots = this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup;
            jobData.m_Geometries = this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup;
            jobData.m_PollutionMap = this.m_GroundPollutionSystem.GetMap(readOnly: true, out var dependencies);
            jobData.m_PollutionParameters = this.m_PollutionParameterQuery.GetSingleton<PollutionParameterData>();
            jobHandle = JobChunkExtensions.ScheduleParallel(jobData, this.m_EdgeGroup, JobHandle.CombineDependencies(base.Dependency, dependencies));
            this.m_GroundPollutionSystem.AddReader(jobHandle);
        }
        if (!this.m_NodeGroup.IsEmptyIgnoreFilter)
        {
            this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
            NodeUpdateJob jobData2;
            jobData2 = default(NodeUpdateJob);
            jobData2.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
            jobData2.m_NodeType = this.__TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle;
            jobData2.m_ConnectedEdgeType = this.__TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle;
            jobData2.m_Curves = this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup;
            jobData2.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
            jobHandle = JobChunkExtensions.ScheduleParallel(jobData2, this.m_NodeGroup, jobHandle);
        }
        base.Dependency = jobHandle;
    }

    private static float GetDistanceFade(float distance)
    {
        return math.saturate(1f - distance / 500f);//vanilla:2000
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void __AssignQueries(ref SystemState state)
    {
    }

    protected override void OnCreateForCompiler()
    {
        base.OnCreateForCompiler();
        this.__AssignQueries(ref base.CheckedStateRef);
        this.__TypeHandle.__AssignHandles(ref base.CheckedStateRef);
    }

    [Preserve]
    public LandValueSystemMod()
    {
    }
}

