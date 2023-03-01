***MAAS-PlanComplexity
![image](https://user-images.githubusercontent.com/78000769/222256063-1d4b63a3-8825-49f1-95f9-d8da49fb2336.png)
Plan Complexity Analyser
Authors: 
Esa Kuusela, Varian Medical Systems.
Filippo Cozzi. Liceo Scientifico A. Sereni, Luino, Italy


The script executes an analysis of the plan complexity for IMRT and VMAT on a field per field basis and extracts some quantitative metrics from the MLC sequences.
NOTE:  the tool currently works only for c-arm linacs (e.g. TrueBeam) and it is not intended for dual layer MLC based machines like Halcyon.  It will be upgraded soon.

	Average aperture area (in mm2).  It is the MLC defined open field averaged over all the control points in the field.  It is provided with its standard deviation.

	Average aperture per leaf couple (in mm), ALPO:  it is the per-leaf-pair aperture averaged per each control point and over all the control points in the field. It is provided with its standard deviation.


	BA:  it is the beam area weighted on the MU per control point.  BA is defined as:
BA=(∑_j▒〖〖MU〗_j 〖AA〗_j 〗)/MU
Where AAj is the aperture area and j is extended to all the control points.



	BI: beam irregularity. BI is defined as:
BI=(∑_j▒〖〖MU〗_j 〖AI〗_j 〗)/MU

Where AIj is the aperture irregularity defined as:
〖AI〗_j=(〖AP〗_j^2)⁄(4π〖AA〗_j )
And APj is the aperture perimeter per each control point



	BM: beam modulation. BM is efined as:
BM=(∑_j▒〖〖MU〗_j 〖AA〗_j 〗)/(MU∙⋃_j▒〖〖(AA〗_j)〗)

Detailed descriptions and discussion of BA, BI and BM can be found in Du et al [1] and a more general overview in the chapter 8 of [2].


References:
[1] Du W, Cho S H, Zhang X, Hoffman K E and Kudchadker R J 2014 Quantification of beam complexity in intensity-modulated radiation therapy treatment plans Med. Phys. 41 021716

[2]. Das. I. , Sanfilippo N, Fogliata A, Cozzi L.    Intensity modulated radiation therapy. A clinical overview.    IOP Publishing, Bristol, UK, 2020
