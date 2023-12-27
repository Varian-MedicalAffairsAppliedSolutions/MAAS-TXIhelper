# MAAS-TXIhelper

## Total X (X=Body|Marrow|Lymphoid) Irradiation helper, introduced at AAPM 2023
<object data="https://medicalaffairs.varian.com/download/AAPM2023_eposter_TXIHelper.pdf" type="application/pdf" width="700px" height="700px">
    <embed src="https://medicalaffairs.varian.com/download/AAPM2023_eposter_TXIHelper.pdf">
        <p>Please download the poster PDF to view it: <a href="https://medicalaffairs.varian.com/download/AAPM2023_eposter_TXIHelper.pdf">Download PDF</a>.</p>
    </embed>
</object>
<br>

### CT concationation tab: working

Requires Head First Supine superior series with rigid registration to inferior Feet First Supine Series in External Beam Planning
- no plan needed
- default CT slice thickness taken from exisiting scans but can be resampled

### Isocenter Placement tab: not yet working
- will automatically place all beams on full body image set
- will support single isocenter shifts head to foot (X fixed,Y fixed,Z changes)
- will also support additional latereral shifts for wider patients (X changes,Y fixed,Z changes)

### Aperture Rotation: working
- supports either: C series / TrueBeam / Halcyon / Ethos
- copy and paste twice the complete plan created in HFS oritination with desired dose distrunution head to foot
-- in one plan delete all beams/isocenters that will be treated HFS and the the other plan FFS
-- DICOM export the plan that has only the inferior isocenters that is currently in the HFS orientation
-- launch this tool, click the Aperature rotation tab, and selected the exported plan file for aperature rotation
-- after aperature rotation script completes import new plan from subdirectory
--- recalulate dose with fixed MUs
--- create a plan sum with inferior plan
--- dose should be identical to inital completed

### Final Modifications: not yet working
- breaks each isocenter group into seperate plan, treatment appvoes plan and adds needed setup notes
