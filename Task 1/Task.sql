/* 1.	Написать SQL-запрос, который возвращает объем продаж в количественном выражении в разрезе сотрудников за период с 01.10.2013 по 07.10.2013:
●	Фамилия и имя сотрудника;
●	Объем продаж сотрудника.
Данные отсортировать по фамилии и имени сотрудника.
*/

SELECT		CONCAT(slrs.Surname, ' ', slrs.Name)	AS Employee
			,SUM(sls.Quantity)						AS SalesVolume
FROM		dbo.Sales AS sls
INNER JOIN	dbo.Sellers AS slrs
	ON		sls.IDSel = slrs.ID
WHERE		sls.Date >= '2013-10-01'
	AND		sls.Date <= '2013-10-07'
GROUP BY	slrs.Surname, slrs.Name
ORDER BY	slrs.Surname, slrs.Name
GO 

/* 2.	На основании созданного в первом задании запроса написать SQL-запрос, который возвращает процент объема продаж в разрезе сотрудников и продукции за период с 01.10.2013 по 07.10.2013:
●	Наименование продукции;
●	Фамилия и имя сотрудника;
●	Процент продаж сотрудником данного вида продукции (продажи сотрудника данной продукции/общее число продаж данной продукции).
В выборку должна попадать продукция, поступившая за период с 07.09.2013 по 07.10.2013.
Данные отсортировать по наименованию продукции, фамилии и имени сотрудника.
*/

SELECT		sq.Product		
			,sq.Employee		
			,(sq.SalesVolume / sq.TotalProductSales) * 100					AS PercOfProductSales
FROM		(SELECT		p.ID												AS IDProd
						,CONCAT(slrs.Surname, ' ', slrs.Name)				AS Employee
						,p.Name												AS Product
						,SUM(sls.Quantity)									AS SalesVolume
						,SUM(SUM(sls.Quantity)) OVER (PARTITION BY p.Name)	AS TotalProductSales
			FROM		dbo.Sales		AS sls
			INNER JOIN	dbo.Sellers		AS slrs
				ON		sls.IDSel = slrs.ID
			INNER JOIN	dbo.Products	AS p
				ON		sls.IDProd = p.ID
			WHERE		sls.Date >= '2013-10-01'
				AND		sls.Date <= '2013-10-07'
			GROUP BY	p.ID, slrs.Surname, slrs.Name, p.Name
			) AS sq
INNER JOIN	dbo.Arrivals AS a
	ON		a.IDProd = sq.IDProd
WHERE		a.Date >= '2013-09-07'
	AND		a.Date <= '2013-10-07'
GROUP BY	sq.Product, sq.Employee, sq.SalesVolume, sq.TotalProductSales		
ORDER BY	sq.Product, sq.Employee