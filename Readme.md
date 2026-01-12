## Visual Logic
### 1. Open Window / Game Logic Editor
![alt text](image.png)


### 2. Right click create logic node
![alt text](image-1.png)


### 3. Connect Nodes
![alt text](image-2.png)


### 4. Set Target Game Object
![alt text](image-3.png)


### 5. Create Monobehavior script the script must have LgoicStart() function

```c#
public void LogicStart()
{
    return;
}
```
or
```c#
public IEnumerator LogicStart()
{
    yield return null;
}
```
Each Node script always start from LogicStart and move next node when LogicStart is end