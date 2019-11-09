## 粒子系统

**完善官方的“汽车尾气”模拟：**

使用官方资源资源 Vehicle 的 car， 使用 Smoke 粒子系统模拟启动发动、运行、故障等场景效果

从左到右分别为故障，发动，运行，的smoke场景效果

主要通过修改粒子的各个部分来实现不同场景下smoke效果的不同
![在这里插入图片描述](https://imgconvert.csdnimg.cn/aHR0cHM6Ly9iZW50c2FpNy5naXRodWIuaW8vYXNzZXRzL2Fzc2V0cy8xNTcxNjM0MTc2NzY2LnBuZw?x-oss-process=image/format,png)

这里可以通过修改Duration来控制粒子的喷射周期，通过Looping设置是否循环喷射，通过 Start Lifetime和Start Speed分别控制粒子的生命周期和喷射速度。通过Start Color来设置粒子颜色。通过设置MaxParticles来限定一周内发射的例子数,多与此数目停止发射。
![在这里插入图片描述](https://imgconvert.csdnimg.cn/aHR0cHM6Ly9iZW50c2FpNy5naXRodWIuaW8vYXNzZXRzL2Fzc2V0cy8xNTcxNjM0MzU0OTg2LnBuZw?x-oss-process=image/format,png)

Shape属性板用于控制喷射的范围 球体(Sphere) 半球体(HemiSphere)圆锥体 Cone, 盒子(Box)网格(Mesh) 环形(Cricle) 边线(Edge)。 由于这里是汽车尾部气体，我们选用盒型的喷射器模拟汽车尾部粒子效果的形状。

![1571634392942](https://imgconvert.csdnimg.cn/aHR0cHM6Ly9iZW50c2FpNy5naXRodWIuaW8vYXNzZXRzL2Fzc2V0cy8xNTcxNjM0MzkyOTQyLnBuZw?x-oss-process=image/format,png)

Emission属性板用于控制喷射的速度。如果汽车要产生气体状的尾气，则将Emission rate的速率稍微调低。如果要产生喷射状的火焰效果，则将Emission rate的速率调高。

![1571634532587](https://imgconvert.csdnimg.cn/aHR0cHM6Ly9iZW50c2FpNy5naXRodWIuaW8vYXNzZXRzL2Fzc2V0cy8xNTcxNjM0NTMyNTg3LnBuZw?x-oss-process=image/format,png)

Renderer属性板用于控制渲染的效果。如果这里要产生黑烟或者火焰，则选取对应的smoke贴图将其作为material属性，以更改喷射效果的贴图。

![https://bentsai7.github.io/assets/assets/1571634683526.png](https://imgconvert.csdnimg.cn/aHR0cHM6Ly9iZW50c2FpNy5naXRodWIuaW8vYXNzZXRzL2Fzc2V0cy8xNTcxNjM0NjgzNTI2LnBuZw?x-oss-process=image/format,png)

Size over Lifetime和Color over Lifetime属性板用于控制粒子随时间变换的颜色和大小。
