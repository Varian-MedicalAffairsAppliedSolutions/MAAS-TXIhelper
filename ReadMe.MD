# MAAS-PlanComplexity

![image](https://user-images.githubusercontent.com/78000769/222256724-04f3dbb9-7178-4f26-8e8c-e25cdaa2c38a.png)

Plan Complexity Analyser
Authors: 
Esa Kuusela, Varian Medical Systems.
Filippo Cozzi. Liceo Scientifico A. Sereni, Luino, Italy


The script executes an analysis of the plan complexity for IMRT and VMAT on a field per field basis and extracts some quantitative metrics from the MLC sequences.
NOTE:  the tool currently works only for c-arm linacs (e.g. TrueBeam) and it is not intended for dual layer MLC based machines like Halcyon.  It will be upgraded soon.

* Average aperture area (in mm2).  It is the MLC defined open field averaged over all the control points in the field.  It is provided with its standard deviation.

* Average aperture per leaf couple (in mm), ALPO:  it is the per-leaf-pair aperture averaged per each control point and over all the control points in the field. It is provided with its standard deviation.

* BA:  it is the beam area weighted on the MU per control point.  BA is defined as:

![image](https://user-images.githubusercontent.com/78000769/222257941-510952a3-c1ea-46cd-a246-e1ad9bbc2960.png)

Where AAj is the aperture area and j is extended to all the control points.

* BI: beam irregularity. BI is defined as:

![image](https://user-images.githubusercontent.com/78000769/222258095-3e811fda-a40a-436c-9708-b5f0c176eb79.png)

Where AIj is the aperture irregularity defined as:

![image](https://user-images.githubusercontent.com/78000769/222258180-e8812303-e411-4d7f-b2ae-a3ede8039ec7.png)

And APj is the aperture perimeter per each control point

* BM: beam modulation. BM is efined as:
* 
![image](https://user-images.githubusercontent.com/78000769/222258323-1fecae2d-8a29-4624-9606-1d3f23261601.png)

Detailed descriptions and discussion of BA, BI and BM can be found in Du et al [1] and a more general overview in the chapter 8 of [2].

References:
[1] Du W, Cho S H, Zhang X, Hoffman K E and Kudchadker R J 2014 Quantification of beam complexity in intensity-modulated radiation therapy treatment plans Med. Phys. 41 021716 https://aapm.onlinelibrary.wiley.com/doi/abs/10.1118/1.4861821

[2]. Das. I. , Sanfilippo N, Fogliata A, Cozzi L.    Intensity modulated radiation therapy. A clinical overview.    IOP Publishing, Bristol, UK, 2020 https://store.ioppublishing.org/page/detail/Intensity-Modulated-Radiation-Therapy//?k=9780750313360
