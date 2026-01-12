-- Usage area
CREATE TABLE usage_area AS
SELECT parameter ->> 'Uuid' AS uuid,parameter ->> 'Bezeich' AS bezeich,
geom AS geom 
FROM geo_data
WHERE parameter_key = 
(
	SELECT id
	FROM geothermal_parameter
	WHERE typeofdata = 'area_usage'
)

--Main
--ax_select WOHN
SELECT (ST_Dump(ST_UNION(parcel_area.geom))).geom AS geom
FROM 
(
	SELECT geom AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'area_usage'
		)
	AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
) AS usable_area,
(
    SELECT geom AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'land_parcels'
	)
) AS parcel_area,
(
	SELECT ST_Union(geom) AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'water_protec_areas'
		)
) AS water_proc_area
WHERE ST_Covers(usable_area.geom, parcel_area.geom)
AND ST_GeometryType(parcel_area.geom) = 'ST_Polygon'
AND ST_Disjoint(water_proc_area.geom, parcel_area.geom)


--Table creation
--- ax_tree
CREATE TABLE ax_tree AS
SELECT tree.geom AS geom
FROM
(
	SELECT geom AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'area_usage'
		)
	AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
) AS usable_area,
(
	SELECT geom AS geom 
    FROM geo_data
    WHERE parameter_key = 35 OR parameter_key = 36
) AS tree
WHERE ST_Covers(usable_area.geom, tree.geom)



--ax_buildings
CREATE TABLE ax_buildings AS
SELECT building.geom AS geom
FROM
(
	SELECT geom AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'area_usage'
		)
	AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
) AS usable_area,
(
	SELECT geom AS geom 
    FROM geo_data
    WHERE parameter_key = 32
) AS building
WHERE ST_Intersects(usable_area.geom, building.geom)











--Main
--ax_select Wohn, Industie und gewerbe
SELECT (ST_Dump(ST_UNION(parcel_area.geom))).geom AS geom
FROM 
(
	SELECT geom AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'area_usage'
		)
    AND (
        parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
        OR parameter ->> 'Bezeich' = 'AX_IndustrieUndGewerbeflaeche'
    )
) AS usable_area,
(
    SELECT geom AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'land_parcels'
	)
) AS parcel_area,
(
	SELECT ST_Union(geom) AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'water_protec_areas'
		)
) AS water_proc_area
WHERE ST_Covers(usable_area.geom, parcel_area.geom)
AND ST_GeometryType(parcel_area.geom) = 'ST_Polygon'
AND ST_Disjoint(water_proc_area.geom, parcel_area.geom)



--Main
/*ax_select Wohn
Industie und gewerbe
Weg
Sport unf reizeit
Platz
FlaecheGemischerNutzung
FlaecheBesondererFunktionaler praegung

*/
SELECT (ST_Dump(ST_UNION(parcel_area.geom))).geom AS geom
FROM 
(
	SELECT geom AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'area_usage'
		)
    AND (
        parameter ->> 'Bezeich' = 'AX_Weg'
        OR parameter ->> 'Bezeich' = 'AX_SportFreizeitUndErholungsflaeche'
		OR parameter ->> 'Bezeich' = 'AX_Platz'
		OR parameter ->> 'Bezeich' = 'AX_IndustrieUndGewerbeflaeche'
		OR parameter ->> 'Bezeich' = 'AX_FlaecheGemischterNutzung'
		OR parameter ->> 'Bezeich' = 'AX_FlaecheBesondererFunktionalerPraegung'
		OR parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
    )
) AS usable_area,
(
    SELECT geom AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'land_parcels'
	)
) AS parcel_area,
(
	SELECT ST_Union(geom) AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'water_protec_areas'
		)
) AS water_proc_area
WHERE ST_Covers(usable_area.geom, parcel_area.geom)
AND ST_GeometryType(parcel_area.geom) = 'ST_Polygon'
AND ST_Disjoint(water_proc_area.geom, parcel_area.geom)















--ax_select Test
CREATE TABLE ax_selected AS
SELECT (ST_Dump(ST_UNION(parcel_area.geom))).geom AS geom
FROM 
(
	SELECT geom AS geom 
	FROM geo_data
	WHERE parameter_key = 
		(
			SELECT id
			FROM geothermal_parameter
			WHERE typeofdata = 'area_usage'
		)
	AND parameter ->> 'Bezeich' = 'AX_Wohnbauflaeche'
	AND
	(
		parameter ->> 'Uuid' = 'DEBE02YY40001cxE'
		OR
		parameter ->> 'Uuid' = 'DEBE02YY40001cxj'
	)
) AS usable_area,
(
    SELECT geom AS geom 
    FROM geo_data
    WHERE parameter_key = 
	(
		SELECT id
		FROM geothermal_parameter
		WHERE typeofdata = 'land_parcels'
	)
) AS parcel_area
WHERE ST_Covers(usable_area.geom, parcel_area.geom)
AND ST_GeometryType(parcel_area.geom) = 'ST_Polygon'




-- possible network distance
SELECT ST_Length(
    ST_LineSubstring(
        line.geom,
        ST_LineLocatePoint(line.geom, source_point.geom),
        ST_LineLocatePoint(line.geom, destination_point.geom)
    )
) AS network_distance
FROM lines AS line, points AS source_point, points AS destination_point
WHERE source_point.id = <source_point_id>
AND destination_point.id = <destination_point_id>;