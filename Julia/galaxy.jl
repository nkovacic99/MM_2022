
using LinearAlgebra;
using DelimitedFiles;

function galaxy(G, dt, stKorakov, zacM)

    mase = zacM[ : , 7];
    n = size(mase, 1);
    println(n)
    izpisna = zeros(n * stKorakov, 6)

    stanje = zeros(n, 9);
    stanje[ : , 1:6]= zacM[ : , 1:6];


    for ix in 1:stKorakov
        stanje[ : , 7:9] = zeros(n, 3)
        izpisna[((ix-1)*n + 1) : ((ix)*n) , 1:6] = stanje[1:n, 1:6]


        # izracun pospeskov
        for i in 1:n
            for j in (i+1):n
                smerVektor = stanje[j, 1:3] - stanje[i, 1:3];
                skalarRazdalje = norm(smerVektor)^3;
                stanje[i, 7:9] += (G * mase[j] /skalarRazdalje) * smerVektor;
                stanje[j, 7:9] += (G * mase[i] /skalarRazdalje) * (-smerVektor);

            end
        end

        #premik
        for i in 1:n
            stanje[i, 1:3] += stanje[i, 4:6] * dt + stanje[i, 7:9] * ((dt^2) / 2);
            stanje[i, 4:6] += stanje[i, 7:9] * dt;
        end



    end




    touch("izpis.txt");
    f = open("izpis.txt", "w");
    writedlm(f, izpisna, ',');




    

    


    


end