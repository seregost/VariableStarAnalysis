-- Stars with no valid FluxB samples
select ID from f_sample where ID not in
(
	select distinct s1.ID from f_sample s1 left join f_sample s2 on s1.ID_Star=s2.ID_Star
	where s1.FluxB=0 and s2.FluxB <> 0
)
and FluxB=0

-- Stars with no valid FluxV samples
select ID from f_sample where ID not in
(
	select distinct s1.ID from f_sample s1 left join f_sample s2 on s1.ID_Star=s2.ID_Star
	where s1.FluxV=0 and s2.FluxV <> 0
)
and FluxV=0