package co.pitboss.erp.comun;

import java.util.Locale;

/** Formato de pesos colombianos: 200000 -> "$200.000". */
public final class Dinero {
    private Dinero() { }

    public static String pesos(long valor) {
        return "$" + String.format(Locale.US, "%,d", Math.abs(valor)).replace(',', '.');
    }
}
