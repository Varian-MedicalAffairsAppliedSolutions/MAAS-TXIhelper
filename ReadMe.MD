# MAAS-TXIhelper

## Total X (X=Body|Marrow|Lymphoid) Irradiation helper, introduced at AAPM 2023
<object data="https://medicalaffairs.varian.com/download/AAPM2023_eposter_TXIHelper.pdf" type="application/pdf" width="700px" height="700px">
    <embed src="https://medicalaffairs.varian.com/download/AAPM2023_eposter_TXIHelper.pdf">
        <p>Please download the poster PDF to view it: <a href="https://medicalaffairs.varian.com/download/AAPM2023_eposter_TXIHelper.pdf">Download PDF</a>.</p>
    </embed>
</object>

### CT concatenation tab: working
![plot](./CTconcat-screenshot.jpg)

Requires Head First Supine superior series with rigid registration to inferior Feet First Supine Series in External Beam Planning
- no plan needed
- default CT slice thickness taken from existing scans but can be resampled
- progress bar is accurate, could take approximately 30 minutes to complete concatenation
- import fully merged CT series from output directory (i.e C:\Temp) when completed

### Isocenter Placement tab: not yet working
- will automatically place all beams on full body image set
- will support single isocenter shifts head to foot (X fixed,Y fixed,Z changes)
- will also support additional lateral shifts for wider patients (X changes,Y fixed,Z changes)

### Aperture Rotation tab: working
![plot](./aperture-rotation-screenshot.png)

### Bonus: CT Number Override tab: working
![plot](CT-number-override-screenshot.jpg)
- re-process CT series with desity of a structure overridden or "burned-in"

- supports either: C series / TrueBeam / Halcyon / Ethos
- copy and paste twice the complete plan created in HFS orientation with desired dose distribution head to foot
    - in one plan delete all beams/isocenters that will be treated HFS and the other plan FFS
    - DICOM export the plan that has only the inferior isocenters that is currently in the HFS orientation
- launch this tool, click the Aperture Rotation tab, and selected the exported plan file for aperture rotation
    - after aperture rotation script completes import new plan from subdirectory
    - calculate dose with fixed MUs
    - create a plan sum with superior plan
    - dose should be identical to initial completed plan

### Final Modifications tab: not yet working
- breaks each isocenter group into separate plan, treatment approves plans and adds needed setup notes

